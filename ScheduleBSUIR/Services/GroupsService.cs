using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;

namespace ScheduleBSUIR.Services
{
    public class GroupsService(WebService webService, DbService dbService, ILoggingService loggingService)
    {
        private readonly WebService _webService = webService;
        private readonly DbService _dbService = dbService;
        private readonly ILoggingService _loggingService = loggingService;

        public async Task<IEnumerable<StudentGroupHeader>> GetGroupHeadersAsync(CancellationToken cancellationToken)
        {
            IEnumerable<StudentGroupHeader>? headers;

            var cachedHeaders = _dbService.GetAll<StudentGroupHeader>();

            bool offline = Connectivity.NetworkAccess is NetworkAccess.None;

            if (offline)
            {
                if (cachedHeaders is null)
                {
                    throw new FileNotFoundException("No cached groups list found for offline access");
                }

                headers = cachedHeaders;
            }
            // Obtain from api and cache if we are connected
            else
            {
                _loggingService.LogInfo($"Obtaining groups list from web api", displayCaller: false);

                headers = await _webService.GetGroupHeadersAsync(string.Empty, cancellationToken);

                if (headers is null)
                {
                    throw new ArgumentException("Couldn't obtain groups from web service");
                }

                // No need to update AccessedAt and other properties for groups list since not using them

                _dbService.AddOrUpdate(headers);
            }

            return headers ?? [];
        }
    }
}
