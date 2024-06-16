namespace ScheduleBSUIR.Interfaces
{
    interface IDateTimeProvider
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }
}
