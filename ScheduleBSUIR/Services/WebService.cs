using ScheduleBSUIR.Helpers;
using ScheduleBSUIR.Helpers.JsonConverters;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.API;
using System.Diagnostics;
using System.Text.Json;

namespace ScheduleBSUIR.Services
{
    public class WebService
    {
        private readonly JsonSerializerOptions _deserializeOptions;
        public WebService()
        { 
            _deserializeOptions = new()
            {
                PropertyNameCaseInsensitive = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            _deserializeOptions.Converters.Add(new JsonDateTimeConverter());
        }

        public async Task<LastUpdateResponse> GetTimetableLastUpdateAsync(TypedId id, CancellationToken cancellationToken)
        {
            var requestUrl = id switch
            {
                StudentGroupId studentGroupId => UrlGenerator.StudentGroupTimetableLastUpdate(studentGroupId),
                EmployeeId employeeId => UrlGenerator.EmployeeTimetableLastUpdate(employeeId),
                _ => throw new UnreachableException()
            };

            return await GetDeserializedDataAsync<LastUpdateResponse>(requestUrl, cancellationToken);
        }

        public async Task<Timetable?> GetTimetableAsync(TypedId id, CancellationToken cancellationToken)
        {
            var requestUrl = id switch
            {
                StudentGroupId studentGroupId => UrlGenerator.StudentGroupTimetable(studentGroupId),
                EmployeeId employeeId => UrlGenerator.EmployeeTimetable(employeeId),
                _ => throw new UnreachableException()
            };

            return await GetDeserializedDataAsync<Timetable>(requestUrl, cancellationToken);
        }

        public async Task<IEnumerable<StudentGroupHeader>?> GetGroupHeadersAsync(string groupNameFilter, CancellationToken cancellationToken)
        {
            var requestUrl = UrlGenerator.GroupsHeaders(groupNameFilter);

            return await GetDeserializedDataAsync<IEnumerable<StudentGroupHeader>>(requestUrl, cancellationToken);
        }

        private async Task<T?> GetDeserializedDataAsync<T>(string url, CancellationToken cancellationToken)
        {
            using var client = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(5),
            };

            var response = await client.GetAsync(url, cancellationToken);

            var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

            T? result = default;
            
            try
            {
                result = await JsonSerializer.DeserializeAsync<T>(responseStream, _deserializeOptions, cancellationToken);
            }
            catch(Exception ex)
            {

            }

            return result;
        }
    }
}
