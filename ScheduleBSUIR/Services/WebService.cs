using ScheduleBSUIR.Helpers;
using ScheduleBSUIR.Helpers.JsonConverters;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.API;
using System.Diagnostics;
using System.Text.Json;

namespace ScheduleBSUIR.Services
{
    public class WebService
    {
        private readonly ILoggingService _loggingService;
        private readonly JsonSerializerOptions _deserializeOptions;
        public WebService(ILoggingService loggingService)
        {
            _loggingService = loggingService;

            _deserializeOptions = new()
            {
                PropertyNameCaseInsensitive = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            _deserializeOptions.Converters.Add(new JsonDateTimeConverter());
        }

        public Task<LastUpdateResponse?> GetTimetableLastUpdateAsync(TypedId id, CancellationToken cancellationToken)
        {
            var requestUrl = id switch
            {
                StudentGroupId studentGroupId => UrlGenerator.StudentGroupTimetableLastUpdate(studentGroupId),
                EmployeeId employeeId => UrlGenerator.EmployeeTimetableLastUpdate(employeeId),
                _ => throw new UnreachableException()
            };

            return GetDeserializedDataAsync<LastUpdateResponse>(requestUrl, cancellationToken);
        }

        public Task<Timetable?> GetTimetableAsync(TypedId id, CancellationToken cancellationToken)
        {
            var requestUrl = id switch
            {
                StudentGroupId studentGroupId => UrlGenerator.StudentGroupTimetable(studentGroupId),
                EmployeeId employeeId => UrlGenerator.EmployeeTimetable(employeeId),
                _ => throw new UnreachableException()
            };

            return GetDeserializedDataAsync<Timetable>(requestUrl, cancellationToken);
        }

        public Task<IEnumerable<StudentGroupHeader>?> GetGroupHeadersAsync(string groupNameFilter, CancellationToken cancellationToken)
        {
            var requestUrl = UrlGenerator.GroupsHeaders(groupNameFilter);

            return GetDeserializedDataAsync<IEnumerable<StudentGroupHeader>>(requestUrl, cancellationToken);
        }

        public Task<IEnumerable<Employee>?> GetEmployeesAsync(CancellationToken cancellationToken)
        {
            var requestUrl = UrlGenerator.Employees();

            return GetDeserializedDataAsync<IEnumerable<Employee>>(requestUrl, cancellationToken);
        }

        private async Task<T?> GetDeserializedDataAsync<T>(string url, CancellationToken cancellationToken)
        {
            T? result = default;

            using var client = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(5),
            };

            try
            {
                var response = await client.GetAsync(url, cancellationToken);

                var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

                result = await JsonSerializer.DeserializeAsync<T>(responseStream, _deserializeOptions, cancellationToken);
            }
            catch (TimeoutException timeoutException)
            {
                _loggingService.LogError($"{nameof(GetDeserializedDataAsync)} timed out: {timeoutException.Message}", displayCaller: false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"{nameof(GetDeserializedDataAsync)} failed with exception: {ex.Message}", displayCaller: false);
            }

            return result;
        }
    }
}
