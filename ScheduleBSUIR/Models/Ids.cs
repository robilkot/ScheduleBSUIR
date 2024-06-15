namespace ScheduleBSUIR.Models
{
    public abstract class TypedId(string Id)
    {
        public override string ToString() => Id;
    }
    public sealed class StudentGroupId(string Id) : TypedId(Id);
    public sealed class EmployeeId(string Id) : TypedId(Id);
}
