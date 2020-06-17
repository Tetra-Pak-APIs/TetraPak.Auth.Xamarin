using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TetraPak.Auth.Xamarin.common
{
    // todo Consider moving MemoryCache<T> to a common NuGet package to be referenced instead
    /// <summary>
    ///   A basic memory cached that supports item longevity.
    /// </summary>
    /// <typeparam name="T">
    ///   Type of items stored by the cache.
    /// </typeparam>
    public class MemoryCache<T>
    {
        readonly object _syncRoot = new object();
        Dictionary<string, CachedItem<T>> _items = new Dictionary<string, CachedItem<T>>();
        Task _cleanTask;

        /// <summary>
        ///   A default key. This is useful for single-use caching.
        ///   When set, the 'key' parameter is not needed for invoking methods.
        /// </summary>
        public string DefaultKey { get; private set; }

        /// <summary>
        ///   Gets the number of cached items.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_syncRoot)
                    return _items.Count;
            }
        }

        /// <summary>
        ///   Adds an item to the cache.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expires"></param>
        /// <returns></returns>
        public virtual async Task AddAsync(T value, DateTime? expires = null)
            => await AddAsync(DefaultKey, value, expires);

        /// <summary>
        ///   Adds an item to the cache.
        /// </summary>
        /// <param name="key">
        ///   (optional)<br/>
        ///   Identifies the item to be added.
        /// </param>
        /// <param name="item">
        ///   The item to be added.
        /// </param>
        /// <param name="expires">
        ///   (optional)<br/>
        ///   A time when the tokens expire. After expiration the item will become
        ///   unavailable after this time.
        /// </param>
        public virtual async Task AddAsync(string key, T item, DateTime? expires = null)
        {
            key = key ?? DefaultKey;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (await ContainsAsync(key))
                throw new ArgumentException($"Key already cached: \"{key}\".", nameof(key));

            lock (_syncRoot)
                _items.Add(key, new CachedItem<T>(key, item, expires ?? DateTime.MaxValue));
        }

        /// <summary>
        ///   Gets a specified item from the cache.
        /// </summary>
        /// <param name="key">
        ///   (optional; default = <see cref="DefaultKey"/>)<br/>
        ///   Identifies the item.
        /// </param>
        /// <returns>
        ///   A <see cref="BoolValue{T}"/> to indicate success and hold the
        ///   requested item.
        /// </returns>
        public virtual Task<BoolValue<T>> TryGetAsync(string key = null)
        {
            key = key ?? DefaultKey;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            lock (_syncRoot)
            {
                if (_items.TryGetValue(key, out CachedItem<T> item) && !item.IsExpired)
                {
                    return Task.FromResult(BoolValue<T>.Success(item.Value));
                }
            }

            return Task.FromResult(BoolValue<T>.Fail());
        }

        /// <summary>
        ///   Returns a value indicating whether a specified item is cached.
        /// </summary>
        /// <param name="key">
        ///   (optional; default = <see cref="DefaultKey"/>)<br/>
        ///   Identifies the item.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the item is cached (and available); otherwise <c>false</c>.
        /// </returns>
        public virtual Task<bool> ContainsAsync(string key = null)
        {
            key = key ?? DefaultKey;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            lock (_syncRoot)
                return Task.FromResult(_items.TryGetValue(key, out CachedItem<T> item) && !item.IsExpired);
        }

        /// <summary>
        ///   Removes all expired items in the background.
        /// </summary>
        public Task CleanAsync()
        {
            if (_cleanTask != null)
                return _cleanTask;

            return _cleanTask = Task.Run(cleanInBackground);
        }

        /// <summary>
        ///   Removes a specified item from cache.
        /// </summary>
        /// <param name="key">
        ///   (optional; default = <see cref="DefaultKey"/>)<br/>
        ///   Identifies the item to be removed.
        /// </param>
        public virtual async Task RemoveAsync(string key = null)
        {
            key = key ?? DefaultKey;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (!await ContainsAsync(key))
                return;

            lock (_syncRoot)
            {
                _items.Remove(key);
            }
        }

        void cleanInBackground()
        {
            List<CachedItem<T>> items;
            lock (_syncRoot)
            {
                items = _items.Values.ToList();
            }

            foreach (var item in items.ToArray())
            {
                if (item.IsExpired)
                    items.Remove(item);
            }
            lock (_syncRoot)
                _items = items.ToDictionary((i) => i.Key);
        }

        /// <summary>
        ///   Assigns a default key for all tokens.
        ///   When set, no key needs to be passed to other methods.
        /// </summary>
        /// <param name="key">
        ///   Identifies cached tokens (typically a client/app id).
        /// </param>
        /// <returns>
        ///   The <see cref="TokenCache"/> instance (fluid API).
        /// </returns>
        public MemoryCache<T> WithDefaultKey(string key)
        {
            DefaultKey = key;
            return this;
        }

        struct CachedItem<TValue>
        {
            DateTime Expires { get; }
            
            public string Key { get; }
            public bool IsExpired => DateTime.Now >= Expires;
            public TValue Value { get; }

            public CachedItem(string key, TValue value, DateTime expires)
            {
                Key = key;
                Expires = expires;
                Value = value;
            }
        }
    }
}
