﻿using LiteDB;
using MethodTimer;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using System.Linq.Expressions;

namespace ScheduleBSUIR.Services
{
    // todo: exception handling in methods with tcs
    public class DbService
    {
        private const string DatabaseFilename = "ScheduleBSUIR.db";
        private readonly LiteDatabase _database;
        private readonly ILoggingService _loggingService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly PreferencesService _preferencesService;

        public DbService(ILoggingService loggingService, IDateTimeProvider dateTimeProvider, PreferencesService preferencesService)
        {
            _loggingService = loggingService;
            _dateTimeProvider = dateTimeProvider;
            _preferencesService = preferencesService;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

            _database = new LiteDatabase(databasePath);



            bool firstInstallForVersion = VersionTracking.Default.IsFirstLaunchForCurrentVersion;

            if (firstInstallForVersion)
            {
                _ = ClearDatabase();
            }
            else
            {
                _ = ClearCacheIfNeeded();
            }

        }

        [Time]
        private async Task ClearCacheIfNeeded()
        {
            double clearInterval = _preferencesService.GetClearCacheInterval();

            // Clearing is disabled
            if (clearInterval == 0)
            {
                return;
            }

            DateTime dateToClear = _dateTimeProvider.Now - TimeSpan.FromDays(clearInterval);

            // Last clear was too recently

            var lastClearDate = _preferencesService.GetClearCacheLastDate();

            if (lastClearDate is not null && lastClearDate > dateToClear)
            {
                _loggingService.LogInfo($"ClearCacheIfNeeded no clearing needed");
                return;
            }

            DeleteStaleObjects<Timetable>();

            await RemoveAllAsync<Employee>();
            await RemoveAllAsync<StudentGroupHeader>();

            _database.Rebuild();

            _database.Commit();

            void DeleteStaleObjects<T>() where T : IUpdateDateAware
            {
                var collection = _database.GetCollection<T>();

                Expression<Func<T, bool>> deletePredicate = (obj) => obj.AccessedAt < dateToClear;

                collection.DeleteMany(deletePredicate);
            }

            _preferencesService.SetClearCacheLastDate(_dateTimeProvider.Now);
        }

        public async Task ClearDatabase()
        {
            await RemoveAllAsync<StudentGroupTimetableHeader>();
            await RemoveAllAsync<EmployeeTimetableHeader>();
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

                //_loggingService.LogInfo($"AddOrUpdate<{typeof(T).Name}> {newObject.PrimaryKey}");

                tcs.SetResult();
            });

            return tcs.Task;
        }

        [Time]
        public Task AddOrUpdateAsync<T>(IEnumerable<T> newObjects) where T : ICacheable
        {
            TaskCompletionSource tcs = new();

            Task.Run(() =>
            {
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

                _loggingService.LogInfo($"AddOrUpdate<{typeof(T).Name}> {newObjects.Count()} objects");

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

                //_loggingService.LogInfo($"Get<{typeof(T).Name}> {primaryKey}");

                tcs.SetResult(result);
            });

            return tcs.Task;
        }

        [Time]
        public Task<List<T>> GetAllAsync<T>() where T : ICacheable
        {
            TaskCompletionSource<List<T>> tcs = new();

            _ = Task.Run(() =>
            {
                var collection = _database.GetCollection<T>();

                var result = collection.FindAll().ToList();

                _loggingService.LogInfo($"GetAll<{typeof(T).Name}> {collection.Count()} objects");

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

                //_loggingService.LogInfo($"Remove<{typeof(T).Name}> {primaryKey}");

                tcs.SetResult();
            });

            return tcs.Task;
        }

        [Time]
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

                _loggingService.LogInfo($"Remove<IEnumerable<{typeof(T).Name}>> {objects.Count()} objects");

                tcs.SetResult();
            });

            return tcs.Task;
        }

        [Time]
        public Task RemoveAllAsync<T>() where T : ICacheable
        {
            TaskCompletionSource tcs = new();

            _ = Task.Run(() =>
            {
                var collection = _database.GetCollection<T>();

                collection.DeleteAll();

                _database.Commit();

                _loggingService.LogInfo($"RemoveAll<{typeof(T).Name}>");

                tcs.SetResult();
            });

            return tcs.Task;
        }
    }
}