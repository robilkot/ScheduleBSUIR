namespace ScheduleBSUIR.Models.DB
{
    public interface ICacheable
    {
        public string PrimaryKey { get; }
        public DateTime UpdatedAt { get; set; }
    }
}
