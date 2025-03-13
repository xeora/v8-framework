using System;
using System.Collections.Generic;
using Xeora.Web.Directives.Elements;

namespace Xeora.Web.Directives
{
    internal class PartialCacheObject
    {
        private readonly List<IDirective> _Directives;
        
        public PartialCacheObject(string cacheId, List<IDirective> directives)
        {
            this.CacheId = cacheId;
            this.Directives = directives;
        }

        public static string CreateUniqueCacheId(int positionId, string cacheIdExtension, PartialCache partialCache, ref Basics.Domain.IDomain instance)
        {
            ArgumentNullException.ThrowIfNull(partialCache);

            if (string.IsNullOrEmpty(instance.Languages.Current.Info.Id) || 
                string.IsNullOrEmpty(partialCache.TemplateTree) || positionId == -1)
                throw new Exceptions.ParseException();

            return $"{instance.Languages.Current.Info.Id}_{partialCache.TemplateTree}_{positionId}_{cacheIdExtension}";
        }

        public string CacheId { get; }
        public List<IDirective> Directives { get; }
    }
}