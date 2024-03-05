namespace ScheduleBSUIR.Services
{
    public record TypedId(string Id);
    public record StudentGroupId(string Id) : TypedId(Id);
    public record EmployeeId(string Id) : TypedId(Id);
}
