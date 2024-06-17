using LiteDB;
using ScheduleBSUIR.Models.DB;

namespace ScheduleBSUIR.Services
{
    public class DbService
    {
        private const string DatabaseFilename = "ScheduleBSUIR.db";
        private readonly LiteDatabase _database;

        public DbService()
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

#if DEBUG
            File.Delete(databasePath);
#endif

            _database = new LiteDatabase(databasePath);
        }

        public void AddOrUpdate<T>(T newObject) where T : ICacheable, new()
        {
            var collection = _database.GetCollection<T>();

            if (collection.Exists(Query.EQ("_id", newObject.PrimaryKey)))
            {
                collection.Update(newObject);
            }
            else
            {
                collection.Insert(newObject);
            }
        }

        public void AddOrUpdatec<T>(List<T> newObjects) where T : ICacheable, new()
        {
            foreach (var newObject in newObjects)
            {
                AddOrUpdate(newObject);
            }
        }

        public T? Get<T>(Guid primaryKey) where T : ICacheable, new()
        {
            var collection = _database.GetCollection<T>();
            return collection.FindById(primaryKey);
        }

        public List<T> GetAll<T>() where T : ICacheable, new()
        {
            var collection = _database.GetCollection<T>();
            return collection.FindAll().ToList();
        }

        public void Remove<T>(T obj) where T : ICacheable
        {
            var collection = _database.GetCollection<T>();
            collection.Delete(obj.PrimaryKey);
        }

        public void Remove<T>(Guid primaryKey) where T : ICacheable, new()
        {
            var collection = _database.GetCollection<T>();
            collection.Delete(primaryKey);
        }

        public void Remove<T>(List<T> objects) where T : ICacheable
        {
            var collection = _database.GetCollection<T>();

            foreach (var obj in objects)
            {
                collection.Delete(obj.PrimaryKey);
            }
        }

        public void RemoveAll<T>() where T : ICacheable, new()
        {
            var collection = _database.GetCollection<T>();
            collection.DeleteAll();
        }
    }
}