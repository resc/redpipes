using System.Collections.Generic;

namespace RedPipes.Configuration.Visualization
{
    /// <summary> Extension methods for <see cref="IDictionary{TKey,TValue}"/> </summary>
    public static class DictionaryExtensions
    {
        /// <summary> REturns the value for the given <paramref name="key"/>, or the specified <paramref name="defaultValue"/>  </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            return dict.TryGetValue(key, out var v) ? v : defaultValue;
        }
    }
}
