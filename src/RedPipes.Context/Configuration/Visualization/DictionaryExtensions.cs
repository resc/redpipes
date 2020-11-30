using System.Collections.Generic;

namespace RedPipes.Configuration.Visualization
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            return dict.TryGetValue(key, out var v) ? v : defaultValue;
        }
    }
}
