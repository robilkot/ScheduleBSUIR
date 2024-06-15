using ScheduleBSUIR.Helpers;
using ScheduleBSUIR.Models;
using System.Diagnostics;
using System.Text.Json;

namespace ScheduleBSUIR.Services
{
    public class WebService
    {
        private static readonly JsonSerializerOptions s_deserializeOptions = new()
        {
            PropertyNameCaseInsensitive = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        // todo: method to get timetable change date
        
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

        private async static Task<T?> GetDeserializedDataAsync<T>(string url, CancellationToken cancellationToken)
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
                result = await JsonSerializer.DeserializeAsync<T>(responseStream, s_deserializeOptions, cancellationToken);
            }
            catch(Exception ex)
            {

            }

            return result;
        }
    }
}
