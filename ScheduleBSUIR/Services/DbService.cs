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

            _database = new LiteDatabase(databasePath);
        }

        public async Task ClearDatabase()
        {
            await RemoveAllAsync<Timetable>();
            await RemoveAllAsync<Employee>();
            await RemoveAllAsync<StudentGroupHeader>();
        }
        public Task AddOrUpdateAsync<T>(T newObject) where T : ICacheable
        {
            TaskCompletionSource tcs = new();

            Task.Run(() =>
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

                //_loggingService.LogInfo($"DbService updated obj with key {newObject.PrimaryKey}", displayCaller: false);

                tcs.SetResult();
            });

            return tcs.Task;
        }

        public Task AddOrUpdateAsync<T>(IEnumerable<T> newObjects) where T : ICacheable
        {
            TaskCompletionSource tcs = new();

            Task.Run(() =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                var collection = _database.GetCollection<T>();

                foreach (var newObject in newObjects)
                {
                    if (collection.Exists(Query.EQ("_id", newObject.PrimaryKey)))
                    {
                        collection.Update(newObject);
                    }
                    else
                    {
                        collection.Insert(newObject.PrimaryKey, newObject);
                    }
                }

                _database.Commit();

                _loggingService.LogInfo($"AddOrUpdate<T> updated {newObjects.Count()} objects in {stopwatch.Elapsed}");

                tcs.SetResult();
            });

            return tcs.Task;
        }

        public Task<T?> GetAsync<T>(string primaryKey) where T : ICacheable
        {
            TaskCompletionSource<T?> tcs = new();

            _ = Task.Run(() =>
            {
                var collection = _database.GetCollection<T>();

                var result = collection.FindById(primaryKey);

                tcs.SetResult(result);
            });

            return tcs.Task;
        }

        public Task<List<T>> GetAllAsync<T>() where T : ICacheable
        {
            TaskCompletionSource<List<T>> tcs = new();

            _ = Task.Run(() =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                var collection = _database.GetCollection<T>();

                var result = collection.FindAll().ToList();

                _loggingService.LogInfo($"GetAll<T> got {collection.Count()} objects in {stopwatch.Elapsed}");

                tcs.SetResult(result);
            });

            return tcs.Task;
        }

        public Task RemoveAsync<T>(T obj) where T : ICacheable
        {
            return RemoveAsync<T>(obj.PrimaryKey);
        }

        public Task RemoveAsync<T>(string primaryKey) where T : ICacheable
        {
            TaskCompletionSource tcs = new();

            _ = Task.Run(() =>
            {
                var collection = _database.GetCollection<T>();

                collection.Delete(primaryKey);

                _database.Commit();

                tcs.SetResult();
            });

            return tcs.Task;
        }

        public Task RemoveAsync<T>(IEnumerable<T> objects) where T : ICacheable
        {
            TaskCompletionSource tcs = new();

            _ = Task.Run(() =>
            {
                var collection = _database.GetCollection<T>();

                foreach (var obj in objects)
                {
                    collection.Delete(obj.PrimaryKey);
                }

                _database.Commit();

                tcs.SetResult();
            });

            return tcs.Task;
        }

        public Task RemoveAllAsync<T>() where T : ICacheable
        {
            TaskCompletionSource tcs = new();

            _ = Task.Run(() =>
            {
                var collection = _database.GetCollection<T>();

                collection.DeleteAll();

                _database.Commit();

                tcs.SetResult();
            });

            return tcs.Task;
        }
    }
}