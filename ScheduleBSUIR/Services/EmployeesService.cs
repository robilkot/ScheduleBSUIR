using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;

namespace ScheduleBSUIR.Services
{
    public class EmployeesService(WebService webService, DbService dbService, ILoggingService loggingService, IDateTimeProvider dateTimeProvider)
    {
        private readonly WebService _webService = webService;
        private readonly DbService _dbService = dbService;
        private readonly ILoggingService _loggingService = loggingService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        public async Task<IEnumerable<Employee>> GetEmployeesAsync(CancellationToken cancellationToken)
        {
            IEnumerable<Employee>? employees;

            var cachedEmployees = await _dbService.GetAllAsync<Employee>();

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

                // Use first employee in list to check if employees were updated in this day. If so, skip updating in db.
                var firstEmployee = employees.FirstOrDefault();

                if (firstEmployee is null)
                {
                    await _dbService.RemoveAllAsync<Employee>();
                }
                else
                {
                    var localFirstGroupHeader = await _dbService.GetAsync<Employee>(firstEmployee.PrimaryKey);

                    if (localFirstGroupHeader is null || localFirstGroupHeader.UpdatedAt <= _dateTimeProvider.Now - TimeSpan.FromDays(1))
                    {
                        firstEmployee.UpdatedAt = _dateTimeProvider.Now;

                        await _dbService.AddOrUpdateAsync(employees);
                    }
                }
            }

            return employees ?? [];
        }
    }
}
