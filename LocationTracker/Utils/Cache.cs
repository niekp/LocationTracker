using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LocationTracker.Utils {
    public class Cache : ICache {
		private IMemoryCache memoryCache;
		private readonly IOptions<MemoryCacheOptions> optionsAccessor;

		public Cache(
			IOptions<MemoryCacheOptions> optionsAccessor
		) {
			this.optionsAccessor = optionsAccessor;
			memoryCache = new MemoryCache(optionsAccessor);
        }

		public MemoryCacheEntryOptions GetCacheOption(ICache.ExpirationType ExpirationType = ICache.ExpirationType.Absolute, int Minutes = 10, int Seconds = 0) {
			var timespan = TimeSpan.FromSeconds((60 * Minutes) + Seconds);
			if (ExpirationType == ICache.ExpirationType.Sliding) {
				return new MemoryCacheEntryOptions().SetSlidingExpiration(timespan);
			} else {
				return new MemoryCacheEntryOptions().SetAbsoluteExpiration(timespan);
			}
		}

		public void ClearCache() {
			memoryCache.Dispose();
			memoryCache = new MemoryCache(optionsAccessor);
		}

		public ICacheEntry CreateEntry(object key) {
			return memoryCache.CreateEntry(key);
		}

		public void Dispose() {
			memoryCache.Dispose();
		}

		public void Remove(object key) {
			memoryCache.Remove(key);
		}

		public bool TryGetValue(object key, out object value) {
			return memoryCache.TryGetValue(key, out value);
		}
	}
}
