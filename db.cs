using System;
using System.Threading.Tasks;
using Windows.Storage;

class IdbKeyVal
{
    private readonly string _dbName;
    private readonly string _storeName;
    private readonly int _version;

    private IdbKeyVal(string dbName = "auth-client-db", string storeName = "ic-keyval", int version = 1)
    {
        _dbName = dbName;
        _storeName = storeName;
        _version = version;
    }

    public static async Task<IdbKeyVal> Create(string dbName = "auth-client-db", string storeName = "ic-keyval", int version = 1)
    {
        // Clear legacy stored keys
        if (ApplicationData.Current.LocalSettings.Values.ContainsKey(KEY_STORAGE_DELEGATION))
        {
            ApplicationData.Current.LocalSettings.Values.Remove(KEY_STORAGE_DELEGATION);
            ApplicationData.Current.LocalSettings.Values.Remove(KEY_STORAGE_KEY);
        }
        return new IdbKeyVal(dbName, storeName, version);
    }

    public async Task Set<T>(string key, T value)
    {
        ApplicationData.Current.LocalSettings.Values[key] = value;
    }

    public async Task<T> Get<T>(string key)
    {
        if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
        {
            return (T)ApplicationData.Current.LocalSettings.Values[key];
        }
        return default(T);
    }

    public async Task Remove(string key)
    {
        ApplicationData.Current.LocalSettings.Values.Remove(key);
    }
}
