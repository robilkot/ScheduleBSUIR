using ScheduleBSUIR.Helpers;
using ScheduleBSUIR.Helpers.JsonConverters;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.API;
using System.Collections.Immutable;
using System.Text.Json;

namespace ScheduleBSUIR.Services.Mock
{
    public class MockWebService : IWebService
    {
        private readonly ILoggingService _loggingService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly JsonSerializerOptions _deserializeOptions;

        private const string TestDataPath = "MockData/Responses/";

        private readonly ImmutableDictionary<string, string> _urlsToFilenames = new Dictionary<string, string>()
        {
            { "https://iis.bsuir.by/api/v1/schedule?studentGroup=000001", $"{TestDataPath}Timetable000001.json" },
            { "https://iis.bsuir.by/api/v1/schedule?studentGroup=000003", $"{TestDataPath}Timetable000003.json" },
            { "https://iis.bsuir.by/api/v1/schedule?studentGroup=221701", $"{TestDataPath}Timetable221701.json" },
            { "https://iis.bsuir.by/api/v1/schedule?studentGroup=221901", $"{TestDataPath}Timetable221901.json" },
            { "https://iis.bsuir.by/api/v1/schedule?studentGroup=210271", $"{TestDataPath}Timetable210271.json" },
            { "https://iis.bsuir.by/api/v1/schedule?studentGroup=314351", $"{TestDataPath}Timetable314351.json" },
            { "https://iis.bsuir.by/api/v1/schedule?studentGroup=122471", $"{TestDataPath}Timetable122471.json" },
            { "https://iis.bsuir.by/api/v1/schedule?studentGroup=350541", $"{TestDataPath}Timetable350541.json" },

            { "https://iis.bsuir.by/api/v1/employees/schedule/v-chueshov", $"{TestDataPath}Timetablev-chueshov.json" },
            { "https://iis.bsuir.by/api/v1/employees/schedule/ts-ma", $"{TestDataPath}Timetablets-ma.json" },

            { "https://iis.bsuir.by/api/v1/student-groups/filters?name=", $"{TestDataPath}MockGroupsHeaders.json" },
            { "https://iis.bsuir.by/api/v1/employees/all", $"{TestDataPath}MockEmployees.json" },
        }
        .ToImmutableDictionary();

        public MockWebService(ILoggingService loggingService, IDateTimeProvider dateTimeProvider)
        {
            _loggingService = loggingService;
            _dateTimeProvider = dateTimeProvider;

            _deserializeOptions = new()
            {
                PropertyNameCaseInsensitive = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            _deserializeOptions.Converters.Add(new JsonDateTimeConverter());
        }

        public Task<LastUpdateResponse?> GetTimetableLastUpdateAsync(TypedId id, CancellationToken cancellationToken)
        {
            var response = new LastUpdateResponse(_dateTimeProvider.Now.Date.AddDays(-2));

            return Task.FromResult<LastUpdateResponse?>(response);
        }

        public Task<Timetable?> GetTimetableAsync(TypedId id, CancellationToken cancellationToken)
        {
            string requestUrl = UrlGenerator.Timetable(id);

            return GetDeserializedDataAsync<Timetable>(requestUrl, cancellationToken);
        }

        public Task<IEnumerable<StudentGroupHeader>?> GetGroupHeadersAsync(string groupNameFilter, CancellationToken cancellationToken)
        {
            string requestUrl = UrlGenerator.GroupsHeaders(groupNameFilter);

            return GetDeserializedDataAsync<IEnumerable<StudentGroupHeader>>(requestUrl, cancellationToken);
        }

        public Task<IEnumerable<Employee>?> GetEmployeesAsync(CancellationToken cancellationToken)
        {
            var requestUrl = UrlGenerator.Employees();

            return GetDeserializedDataAsync<IEnumerable<Employee>>(requestUrl, cancellationToken);
        }

        public Task<int?> GetCurrentWeekAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<int?>(2);
        }

        private async Task<T?> GetDeserializedDataAsync<T>(string url, CancellationToken cancellationToken)
        {
            T? result = default;

            try
            {
                var stream = await FileSystem.OpenAppPackageFileAsync(_urlsToFilenames[url]);

                result = await JsonSerializer.DeserializeAsync<T>(stream, _deserializeOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"{nameof(GetDeserializedDataAsync)} failed:\n{ex.Message}", displayCaller: false);
            }

            return result;
        }
    }
}
