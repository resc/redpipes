using System.Threading;

namespace RedPipes
{
    /// <summary>
    /// Provides context to operation execution, The <see cref="Token"/> is used to signal cancellation,
    /// and <see cref="TryGetValue{T}"/> is used to fetch stored information that is relevant to the current operation from the context.
    /// </summary>
    /// <remarks>
    /// The context should not be used as a kind of dynamic dependency injection/service locator,
    /// but rather to store things like request ids and authentication info for the user requesting the operation. 
    /// </remarks>
    public interface IContext
    {
        /// <summary> The cancellation token </summary>
        CancellationToken Token { get; }

        /// <summary> Retrieve stored values from the context </summary>
        /// <typeparam name="T">The type of the stored value, if the value's type is not an exact match, <see cref="TryGetValue{T}"/> returns false</typeparam>
        /// <param name="key">The key under which the value is stored</param>
        /// <param name="value">The retrieved value</param>
        /// <returns>True if the key is associated with a value of type <typeparamref name="T" />, false if the key is null, or the key is not known</returns>
        bool TryGetValue<T>(object key, out T value);
         
    }
}
