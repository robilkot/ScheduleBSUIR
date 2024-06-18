using LiteDB;
using ScheduleBSUIR.Models.DB;
using System.Diagnostics;

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
            try
            {
                File.Delete(databasePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"File {databasePath} was NOT deleted: {ex.Message}");
            }
#endif

            _database = new LiteDatabase(databasePath);
        }

        public void AddOrUpdate<T>(T newObject) where T : ICacheable
        {
            var collection = _database.GetCollection<T>();

            if (collection.Exists(Query.EQ("_id", newObject.PrimaryKey)))
            {
                collection.Update(newObject);
            }
            else
            {
                collection.Insert(newObject.PrimaryKey, newObject);
            }

            _database.Commit();
        }

        public void AddOrUpdate<T>(List<T> newObjects) where T : ICacheable
        {
            foreach (var newObject in newObjects)
            {
                AddOrUpdate(newObject);
            }
        }

        public T? Get<T>(string primaryKey) where T : ICacheable
        {
            var collection = _database.GetCollection<T>();

            return collection.FindById(primaryKey);
        }

        public List<T> GetAll<T>() where T : ICacheable
        {
            var collection = _database.GetCollection<T>();

            return collection.FindAll().ToList();
        }

        public void Remove<T>(T obj) where T : ICacheable
        {
            Remove<T>(obj.PrimaryKey);
        }

        public void Remove<T>(string primaryKey) where T : ICacheable
        {
            var collection = _database.GetCollection<T>();

            collection.Delete(primaryKey);

            _database.Commit();
        }

        public void Remove<T>(List<T> objects) where T : ICacheable
        {
            var collection = _database.GetCollection<T>();

            foreach (var obj in objects)
            {
                collection.Delete(obj.PrimaryKey);
            }
        }

        public void RemoveAll<T>() where T : ICacheable
        {
            var collection = _database.GetCollection<T>();

            collection.DeleteAll();

            _database.Commit();
        }
    }
}