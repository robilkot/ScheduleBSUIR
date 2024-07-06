using ScheduleBSUIR.Helpers;
using ScheduleBSUIR.Helpers.JsonConverters;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.API;
using System.Text.Json;

namespace ScheduleBSUIR.Services
{
    public class WebService : IWebService
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

        public Task<LastUpdateResponse?> GetTimetableLastUpdateAsync(TimetableHeader id, CancellationToken cancellationToken)
        {
            string requestUrl = UrlGenerator.TimetableLastUpdate(id);

            return GetDeserializedDataAsync<LastUpdateResponse>(requestUrl, cancellationToken);
        }

        public Task<Timetable?> GetTimetableAsync(TimetableHeader id, CancellationToken cancellationToken)
        {
            string requestUrl = UrlGenerator.Timetable(id);

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

        public Task<int?> GetCurrentWeekAsync(CancellationToken cancellationToken)
        {
            var requestUrl = UrlGenerator.CurrentWeek();

            return GetDeserializedDataAsync<int?>(requestUrl, cancellationToken);
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
                _loggingService.LogError($"{nameof(GetDeserializedDataAsync)} failed:\n{ex.Message}", displayCaller: false);
            }

            return result;
        }
    }
}
