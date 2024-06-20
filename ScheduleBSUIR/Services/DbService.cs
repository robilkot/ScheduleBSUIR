using LiteDB;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.DB;
using System.Diagnostics;

namespace ScheduleBSUIR.Services
{
    public class DbService
    {
        private const string DatabaseFilename = "ScheduleBSUIR.db";
        private readonly LiteDatabase _database;
        private readonly ILoggingService _loggingService;

        public DbService(ILoggingService loggingService)
        {
            _loggingService = loggingService;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

            //#if DEBUG
            //            try
            //            {
            //                File.Delete(databasePath);
            //            }
            //            catch (Exception ex)
            //            {
            //                _loggingService.LogError($"File {databasePath} was NOT deleted: {ex.Message}", displayCaller: false);
            //            }
            //#endif

            _database = new LiteDatabase(databasePath);
        }

        public void ClearDatabase()
        {
            RemoveAll<Timetable>();
            //RemoveAll<StudentGroupHeader>();
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

        public void AddOrUpdate<T>(IEnumerable<T> newObjects) where T : ICacheable
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
            Stopwatch stopwatch = Stopwatch.StartNew();

            var collection = _database.GetCollection<T>();

            var result = collection.FindAll().ToList();

            _loggingService.LogInfo($"GetAll<T> worked in {stopwatch.Elapsed}");

            return result;
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

        public void Remove<T>(IEnumerable<T> objects) where T : ICacheable
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