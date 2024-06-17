using ScheduleBSUIR.Models.DB;

namespace ScheduleBSUIR.Services
{
    public class DbService
    {
        public DbService()
        {

        }

        public void AddOrUpdate<T>(T newObject) where T : ICacheable
        {

        }

        public void AddOrUpdate<T>(List<T> newObjects) where T : ICacheable
        {

        }

        public async Task AddOrUpdateAsync<T>(List<T> newObjects) where T : ICacheable
        {

        }

        public async Task ClearDatabase()
        {

        }

        public T? Get<T>(Guid primaryKey) where T : ICacheable
        {

        }


        public List<T> GetAll<T>() where T : ICacheable
        {

        }

        public void Remove<T>(T obsoleteObject) where T : ICacheable
        {

        }

        public void Remove<T>(Guid primaryKey) where T : ICacheable
        {

        }

        public void Remove<T>(List<T> obsoletObjects) where T : ICacheable
        {

        }

        public void Remove<T>() where T : ICacheable
        {

        }
    }
}