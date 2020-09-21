using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TetraPak.Auth.Xamarin.common
{
    /// <summary>
    ///   This implementation of the <see cref="ICache{T}"/> interface relies on files saved within
    ///   a specified folder of the file system.
    /// </summary>
    public class FileCache<T> : MemoryCache<T>
    {
        readonly TaskCompletionSource<bool> _loadingTcs;
        readonly DirectoryInfo _directory;
        readonly string _fileSuffix;

        public override async Task<bool> ContainsAsync(string key = null)
        {
            await ifPreloading();
            return await base.ContainsAsync(key);
        }

        private async Task ifPreloading()
        {
            if (_loadingTcs.Task.IsActive())
            {
                await _loadingTcs.Task;
            }
        }

        public override async Task<BoolValue<T>> TryGetAsync(string key = null)
        {
            var isLoaded = await base.TryGetAsync(key);
            if (isLoaded)
                return isLoaded;

            var path = Path.Combine(_directory.FullName, $"{key}{_fileSuffix}");
            if (!File.Exists(path))
                return BoolValue<T>.Fail();
            
            var json = File.ReadAllText(path);
            var cachedItem = json.FromJson<T>();
            await base.AddAsync(cachedItem.Key, cachedItem.Value, true, cachedItem.Expires);
            return BoolValue<T>.Success(cachedItem.Value);
        }
        
        public override async Task AddAsync(string key, T item, bool replace = false, DateTime? expires = null)
        {
            /* obsolete 
            if (_loadingTcs.Task.Status != TaskStatus.RanToCompletion)
            {
                await base.AddAsync(key, item, expires);
                return;
            }
            */
            await ifPreloading();
            validateKey(key);
            var path = toPath(key);
            var cachedItem = new CachedItem<object>(key, item, expires ?? DateTime.MaxValue);
            var json = cachedItem.ToJson();
            File.WriteAllText(path, json);
            await base.AddAsync(key, item, replace, expires);
        }

        void validateKey(string key)
        {
            if (key.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
                throw new ArgumentException($"Invalid key: {key}. Only letters, digits and '_' are accepted");
        }

        public override async Task RemoveAsync(string key = null)
        {
            await ifPreloading();
            throw new NotImplementedException();
        }

        protected override Task OnRemoveAsync(CachedItem<T> item)
        {
            var path = toPath(item.Key);
            File.Delete(path);
            return base.OnRemoveAsync(item);
        }

        string toPath(string key) => Path.Combine(_directory.FullName, $"{key}{_fileSuffix}");

        public FileCache(DirectoryInfo directory, string fileSuffix, bool preload)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _fileSuffix = fileSuffix ?? throw new ArgumentNullException(nameof(fileSuffix));
            _loadingTcs = new TaskCompletionSource<bool>();
            if (preload)
            {
                loadFromFileSystemAsync();
                return;
            }
            _loadingTcs.SetCanceled();
        }

        void loadFromFileSystemAsync()
        {
            Task.Run(async () =>
            {
                try
                {
                    var files = _directory.GetFiles($"*{_fileSuffix}");
                    foreach (var fileInfo in files)
                    {
                        var json = File.ReadAllText(fileInfo.FullName);
                        var item = json.FromJson<T>();
                        await AddAsync(item.Key, item.Value, false, item.Expires);
                    }
                    _loadingTcs.SetResult(true);
                }
                catch (Exception ex) 
                {
                    _loadingTcs.SetException(ex);
                }
            });
         }
    }
    
    static class TaskHelper
    {
        public static bool IsActive(this Task self)
        {
            return self.Status < TaskStatus.RanToCompletion;
        }
    }

}