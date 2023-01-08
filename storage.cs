using System.Threading.Tasks;

namespace IdentityModel
{
    public interface IAuthClientStorage
    {
        Task<string> Get(string key);
        Task Set(string key, string value);
        Task Remove(string key);
    }

    public class LocalStorage : IAuthClientStorage
    {
        private readonly string _prefix;
        private readonly ISessionStorage _sessionStorage;

        public LocalStorage(string prefix = "ic-", ISessionStorage sessionStorage = null)
        {
            _prefix = prefix;
            _sessionStorage = sessionStorage;
        }

        public Task<string> Get(string key)
        {
            return Task.FromResult(_sessionStorage?.GetItem(_prefix + key) ?? null);
        }

        public Task Set(string key, string value)
        {
            _sessionStorage?.SetItem(_prefix + key, value);
            return Task.CompletedTask;
        }

        public Task Remove(string key)
        {
            _sessionStorage?.RemoveItem(_prefix + key);
            return Task.CompletedTask;
        }
    }

    public interface ISessionStorage
    {
        string GetItem(string key);
        void SetItem(string key, string value);
        void RemoveItem(string key);
    }

    public class IdbStorage : IAuthClientStorage
    {
        private IdbKeyVal _initializedDb;

        private async Task<IdbKeyVal> GetDb()
        {
            if (_initializedDb != null)
            {
                return _initializedDb;
            }

            _initializedDb = await IdbKeyVal.Create(new { version = DB_VERSION });
            return _initializedDb;
        }

        public async Task<string> Get(string key)
        {
            var db = await GetDb();
            return await db.Get<string>(key);
        }

        public async Task Set(string key, string value)
        {
            var db = await GetDb();
            await db.Set(key, value);
        }

        public async Task Remove(string key)
        {
            var db = await GetDb();
            await db.Remove(key);
        }
    }
}