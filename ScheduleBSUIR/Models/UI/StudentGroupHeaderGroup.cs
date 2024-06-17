namespace ScheduleBSUIR.Models.UI
{
    public class StudentGroupHeaderGroup(string header, IEnumerable<StudentGroupHeader> collection)
        : List<StudentGroupHeader>(collection)
    {
        public string Header { get; set; } = header;
    }
}
