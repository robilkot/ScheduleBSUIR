using ScheduleBSUIR.Model;
using System.Net;
using System.Text.Json;

namespace ScheduleBSUIR.Services
{
    public class TimetableService
    {
        // todo: caching
        public async Task<Timetable> GetTimetable(TypedId id, bool forceUpdate = false)
        {
            var requestUrl = id switch
            {
                StudentGroupId => Constants.Urls.StudentGroupTimetable,
                EmployeeId => Constants.Urls.EmployeeTimetable,
                _ => throw new NotImplementedException("Unknown TypedId-derived type provided"),
            };

            requestUrl = string.Format(requestUrl, id.Id);

            var client = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(5),
            };

            Timetable timetable = null!;

            try
            {
                var json = await client.GetAsync(requestUrl);

                JsonSerializerOptions options = new()
                {
                    PropertyNameCaseInsensitive = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                timetable = await JsonSerializer.DeserializeAsync<Timetable>(json.Content.ReadAsStream(), options)
                    ?? throw new WebException("Couldn't get timetable");
            }
            catch (WebException ex)
            {
                // todo: handle web exceptions
            }
            catch (TaskCanceledException ex)
            {
                // todo: timeout and retry handling
            }

            return timetable;
        }
    }
}
