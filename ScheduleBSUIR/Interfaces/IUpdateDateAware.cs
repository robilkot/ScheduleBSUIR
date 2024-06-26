namespace ScheduleBSUIR.Interfaces
{
    interface IUpdateDateAware
    {
        public DateTime UpdatedAt { get; set; }
        public DateTime AccessedAt { get; set; }
    }
}
