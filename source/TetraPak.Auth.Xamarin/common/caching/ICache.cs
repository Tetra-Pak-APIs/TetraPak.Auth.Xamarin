using System;
using System.Threading.Tasks;

namespace TetraPak.Auth.Xamarin.common
{
    // todo Consider moving ICache<T> to a common NuGet package to be referenced instead
    /// <summary>
    ///   Classes implementing this interface can support the caching of items. 
    /// </summary>
    /// <typeparam name="T">
    ///   The type of item(s) that can be cached by the implementing class.
    /// </typeparam>
    public interface ICache<T>
    {
        /// <summary>
        ///   Useful for situations where an instance of the cache only holds one item. 
        /// </summary>
        string DefaultKey { get; }

        /// <summary>
        ///   Examines the cache and returns a value indicating whether a specified item is currently cached.
        /// </summary>
        /// <param name="key">'
        ///   The key associated with the requested item.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the specified <paramref name="key"/> is present in the cache;
        ///   otherwise <c>false</c>.
        /// </returns>
        Task<bool> ContainsAsync(string key = null);

        /// <summary>
        ///   Looks up and retrieves a cached item.
        /// </summary>
        /// <param name="key">'
        ///   The key associated with the requested item.
        /// </param>
        /// <returns>
        ///   A <see cref="BoolValue{T}"/> indicating the outcome while also holding the
        ///   requested item as its <see cref="BoolValue{T}.Value"/>. 
        /// </returns>
        Task<BoolValue<T>> TryGetAsync(string key = null);

        /// <summary>
        ///   Adds an (or replaces an existing) item in the cache.
        /// </summary>
        /// <param name="key">'
        ///   A key to be associated with the added <paramref name="item"/>.
        /// </param>
        /// <param name="item">
        ///   The item to be cached.   
        /// </param>
        /// <param name="replace">
        ///   (optional; default = <c>false</c>)
        ///   When set, an existing item will be replaced. When unset, and an item is already 
        ///   associated with the <paramref name="key"/>, an <see cref="ArgumentException"/>
        ///   is thrown.
        /// </param>
        /// <param name="expires">
        ///   (optional; default = <see cref="DateTime.MaxValue"/>)
        ///   Specifies when this item should become unavailable and marked for removal (see <see cref="CleanAsync"/>). 
        /// </param>
        Task AddAsync(string key, T item, bool replace = false, DateTime? expires = null);
        
        /// <summary>
        ///   Adds an (or replaces an existing) item in the cache.
        /// </summary>
        /// <param name="item">
        ///   The item to be cached.   
        /// </param>
        /// <param name="replace">
        ///   (optional; default = <c>false</c>)
        ///   When set, an existing item will be replaced. When unset, and an item is already 
        ///   associated with the <see cref="DefaultKey"/>, an <see cref="ArgumentException"/>
        ///   is thrown.
        /// </param>
        /// <param name="expires">
        ///   (optional; default = <see cref="DateTime.MaxValue"/>)
        ///   Specifies when this item should become unavailable and marked for removal (see <see cref="CleanAsync"/>). 
        /// </param>
        Task AddAsync(T item, bool replace = false, DateTime? expires = null);
        
        /// <summary>
        ///   Removes a specified item from the cache.
        /// </summary>
        /// <param name="key">
        ///   Identifies the item to be removed.   
        /// </param>
        /// <returns></returns>
        Task RemoveAsync(string key = null);

        /// <summary>
        ///   Initiates cleaning out all items that have expired. 
        /// </summary>
        Task CleanAsync();
        
        /// <summary>
        ///   A fluent API, useful for initializing a cache while also setting the <see cref="DefaultKey"/>.
        /// </summary>
        /// <param name="key">
        ///   The value to be assigned as the <see cref="DefaultKey"/>.
        /// </param>
        /// <returns>
        ///   The cache instance.
        /// </returns>
        ICache<T> WithDefaultKey(string key);
    }
}