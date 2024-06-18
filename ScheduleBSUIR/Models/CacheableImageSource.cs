using ScheduleBSUIR.Models.DB;

namespace ScheduleBSUIR.Models
{
    public class CacheableImageSource : ImageSource, ICacheable
    {
        public string Source { get; set; } = string.Empty;
        public string PrimaryKey => Source;

        public DateTime UpdatedAt { get; set; }
        public DateTime AccessedAt { get; set; }
    }
}
