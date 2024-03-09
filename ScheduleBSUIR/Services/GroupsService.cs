using ScheduleBSUIR.Model;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ScheduleBSUIR.Services
{
    public class GroupsService
    {
        public async Task<List<StudentGroupHeader>> GetGroupHeaders(string? groupNameFilter = null)
        {
            var requestUrl = Constants.Urls.GroupsHeaders;

            if (!string.IsNullOrEmpty(groupNameFilter))
            {
                requestUrl += groupNameFilter;
            }

            var client = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(5),
            };

            List<StudentGroupHeader> groups = null!;

            try
            {
                var json = await client.GetAsync(requestUrl);

                JsonSerializerOptions options = new()
                {
                    PropertyNameCaseInsensitive = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                groups = await JsonSerializer.DeserializeAsync<List<StudentGroupHeader>>(json.Content.ReadAsStream(), options)
                    ?? throw new WebException("Couldn't get student groups list");
            }
            catch (WebException ex)
            {
                // todo: handle web exceptions
            }
            catch (TaskCanceledException ex)
            {
                // todo: timeout and retry handling
            }

            return groups;
        }
    }
}
