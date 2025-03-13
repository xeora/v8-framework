using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Xeora.Web.Directives
{
    internal class PartialCachePool
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, PartialCacheObject>> _PartialCaches;

        private PartialCachePool() =>
            this._PartialCaches = new ConcurrentDictionary<string, ConcurrentDictionary<string, PartialCacheObject>>();

        private static readonly object Lock = new();
        private static PartialCachePool _current;
        public static PartialCachePool Current
        {
            get
            {
                Monitor.Enter(PartialCachePool.Lock);
                try
                {
                    return PartialCachePool._current ?? (PartialCachePool._current = new PartialCachePool());
                }
                finally
                {
                    Monitor.Exit(PartialCachePool.Lock);
                }
            }
        }

        private string Key(string[] domainIdAccessTree) =>
            string.Join("/", domainIdAccessTree);
        
        public void AddOrUpdate(string[] domainIdAccessTree, PartialCacheObject cacheObject)
        {
            this._PartialCaches.AddOrUpdate(
                this.Key(domainIdAccessTree),
                new ConcurrentDictionary<string, PartialCacheObject>(
                    new[] { new KeyValuePair<string, PartialCacheObject>(cacheObject.CacheId, cacheObject) }
                ),
                (_, cO) =>
                {
                    cO.TryAdd(cacheObject.CacheId, cacheObject);
                    return cO;
                }
            );
        }

        public void Get(string[] domainIdAccessTree, string cacheId, out PartialCacheObject cacheObject)
        {
            cacheObject = null;
            
            if (this._PartialCaches.TryGetValue(this.Key(domainIdAccessTree), out ConcurrentDictionary<string, PartialCacheObject> cacheObjects))
                cacheObjects.TryGetValue(cacheId, out cacheObject);
        }

        public void Reset() =>
            this._PartialCaches.Clear();
    }
}