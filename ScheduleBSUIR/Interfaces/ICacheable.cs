using LiteDB;

namespace ScheduleBSUIR.Interfaces
{
    public interface ICacheable
    {
        [BsonId]
        public string PrimaryKey { get; }
    }
}
