using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValueLee.ObjectModel
{
    public class DataCache : IDataCache
    {
        private readonly Dictionary<string, object> _cache;

        public DataCache()
        {
            _cache = new Dictionary<string, object>();
        }

        public void Add(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            _cache[key] = value;
        }

        public object Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            _cache.TryGetValue(key, out var value);
            return value;
        }
    }
}