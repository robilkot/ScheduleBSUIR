using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;

namespace ScheduleBSUIR.Services
{
    public class GroupsService(WebService webService, DbService dbService, ILoggingService loggingService, IDateTimeProvider dateTimeProvider)
    {
        private readonly WebService _webService = webService;
        private readonly DbService _dbService = dbService;
        private readonly ILoggingService _loggingService = loggingService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        public async Task<IEnumerable<StudentGroupHeader>> GetGroupHeadersAsync(CancellationToken cancellationToken)
        {
            IEnumerable<StudentGroupHeader>? headers;

            var cachedHeaders = await _dbService.GetAllAsync<StudentGroupHeader>();

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

                // Use first group in list to check if groups were updated in this day. If so, skip updating in db.
                var firstGroupHeader = headers.FirstOrDefault();
                
                if(firstGroupHeader is null)
                {
                    await _dbService.RemoveAllAsync<StudentGroupHeader>();
                } 
                else
                {
                    var localFirstGroupHeader = await _dbService.GetAsync<StudentGroupHeader>(firstGroupHeader.PrimaryKey);
                
                    if(localFirstGroupHeader is null || localFirstGroupHeader.UpdatedAt <= _dateTimeProvider.Now - TimeSpan.FromDays(1))
                    {
                        firstGroupHeader.UpdatedAt = _dateTimeProvider.Now;
                        
                        await _dbService.AddOrUpdateAsync(headers);
                    }
                }
            }

            return headers ?? [];
        }
    }
}
