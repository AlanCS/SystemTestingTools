using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MovieProject.Logic.Option;
using System;
using System.Threading.Tasks;

namespace MovieProject.Logic
{
    public interface ICacheWrapper
    {
        Task<T> GetOrSetFromCache<T>(string key, Func<Task<T>> noCacheGenerationFunc);
    }

    public class CacheWrapper : ICacheWrapper
    {
        private TimeSpan? _slidingExpiration = null;
        private IMemoryCache _cache = null;

        public CacheWrapper(IMemoryCache cache, IOptions<Caching> cachingOptions)
        {
            _cache = cache;

            var seconds = cachingOptions?.Value?.movieApiInSeconds ?? 0;
            if (seconds == 0)
                throw new ArgumentException("Invalid value for sliding cache expiration", nameof(cachingOptions)); // default value not acceptable
            if (seconds > 1) // not special value to disable caching
                _slidingExpiration = TimeSpan.FromSeconds(seconds);
        }

        public async Task<T> GetOrSetFromCache<T>(string key, Func<Task<T>> noCacheGenerationFunc)
        {
            T result = default(T);

            if (!_cache.TryGetValue(key, out result))
            {
                // Key not in cache, so get data.
                result = await noCacheGenerationFunc();

                var cacheEntryOptions = new MemoryCacheEntryOptions();

                if (_slidingExpiration == null) // special value to disable caching
                    cacheEntryOptions.SetAbsoluteExpiration(DateTimeOffset.Now); // will effectively not cache, but still run the caching mechanism (good to test)                
                else
                    cacheEntryOptions.SetSlidingExpiration(_slidingExpiration.Value);

                // Save data in cache.
                _cache.Set(key, result, cacheEntryOptions);
            }

            return result;
        }
    }
}
