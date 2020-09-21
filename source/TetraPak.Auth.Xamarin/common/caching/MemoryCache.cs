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
    public class MemoryCache<T> : ICache<T>
    {
        readonly object _syncRoot = new object();
        Dictionary<string, CachedItem<T>> _items = new Dictionary<string, CachedItem<T>>();
        Task _cleanTask;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public virtual async Task AddAsync(T value, bool replace = false, DateTime? expires = null) => await AddAsync(DefaultKey, value, replace, expires);

        /// <inheritdoc/>
        public virtual async Task AddAsync(string key, T item, bool replace = false, DateTime? expires = null)
        {
            key ??= DefaultKey;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                if (!replace && await ContainsAsync(key))
                    throw new ArgumentException($"Key already cached: \"{key}\".", nameof(key));

                lock (_syncRoot)
                    _items[key] = new CachedItem<T>(key, item, expires ?? DateTime.MaxValue);
            }
            catch (Exception ex) // nisse
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual Task<BoolValue<T>> TryGetAsync(string key = null)
        {
            key ??= DefaultKey;
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

        /// <inheritdoc/>
        public virtual Task<bool> ContainsAsync(string key = null)
        {
            key ??= DefaultKey;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            lock (_syncRoot)
                return Task.FromResult(_items.TryGetValue(key, out CachedItem<T> item) && !item.IsExpired);
        }

        /// <inheritdoc/>
        public Task CleanAsync()
        {
            if (_cleanTask != null)
                return _cleanTask;

            return _cleanTask = Task.Run(OnCleanInBackground);
        }

        /// <inheritdoc/>
        public virtual async Task RemoveAsync(string key = null)
        {
            key ??= DefaultKey;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (!await ContainsAsync(key))
                return;

            lock (_syncRoot)
            {
                _items.Remove(key);
            }
        }

        protected virtual async void OnCleanInBackground()
        {
            List<CachedItem<T>> items;
            lock (_syncRoot)
            {
                items = _items.Values.ToList();
            }

            foreach (var item in items.ToArray())
            {
                if (!item.IsExpired)
                    continue;

                await OnRemoveAsync(item);
                items.Remove(item);
            }
            lock (_syncRoot)
                _items = items.ToDictionary((i) => i.Key);
        }

        /// <summary>
        ///   Called internally to remove an item from the cache.
        ///   Override this method to perform the removal logics of a derived cache.
        /// </summary>
        /// <param name="item">
        ///   The item to be removed.
        /// </param>
        protected virtual async Task OnRemoveAsync(CachedItem<T> item)
        { }

        /// <inheritdoc/>
        public ICache<T> WithDefaultKey(string key)
        {
            DefaultKey = key;
            return this;
        }
    }
}
