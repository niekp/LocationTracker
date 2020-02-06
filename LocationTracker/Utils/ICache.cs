using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace LocationTracker.Utils {
    public interface ICache : IMemoryCache {
        MemoryCacheEntryOptions GetCacheOption(ICache.ExpirationType ExpirationType = ICache.ExpirationType.Absolute, int Minuten = 10, int Seconden = 0);
        enum ExpirationType { Sliding, Absolute };
		void ClearCache();
    }
}
