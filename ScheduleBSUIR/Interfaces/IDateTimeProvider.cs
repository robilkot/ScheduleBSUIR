namespace ScheduleBSUIR.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
        Task<int> GetWeekAsync(DateTime date, CancellationToken cancellationToken);
    }
}
