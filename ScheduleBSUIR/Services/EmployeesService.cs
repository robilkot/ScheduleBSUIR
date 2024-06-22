using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;

namespace ScheduleBSUIR.Services
{
    public class EmployeesService(WebService webService, DbService dbService, ILoggingService loggingService)
    {
        private readonly WebService _webService = webService;
        private readonly DbService _dbService = dbService;
        private readonly ILoggingService _loggingService = loggingService;

        public async Task<IEnumerable<Employee>> GetEmployeesAsync(CancellationToken cancellationToken)
        {
            IEnumerable<Employee>? employees;

            var cachedEmployees = _dbService.GetAll<Employee>();

            bool offline = Connectivity.NetworkAccess is NetworkAccess.None;

            if (offline)
            {
                if (cachedEmployees is null)
                {
                    throw new FileNotFoundException("No cached employees list found for offline access");
                }

                employees = cachedEmployees;
            }
            // Obtain from api and cache if we are connected
            else
            {
                _loggingService.LogInfo($"Obtaining employees list from web api", displayCaller: false);

                employees = await _webService.GetEmployeesAsync(cancellationToken);

                if (employees is null)
                {
                    throw new ArgumentException("Couldn't obtain employees from web service");
                }

                // No need to update AccessedAt and other properties for groups list since not using them

                _dbService.AddOrUpdate(employees);
            }

            return employees ?? [];
        }
    }
}
