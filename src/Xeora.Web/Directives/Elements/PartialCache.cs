using System.Collections.Generic;
using Xeora.Web.Global;

namespace Xeora.Web.Directives.Elements
{
    public class PartialCache : Directive
    {
        private const string DYNAMIC_CACHE_POINTER = "!DYNAMIC";
        
        private readonly int _PositionId;
        private readonly string[] _Parameters;
        private readonly string[] _CacheIdExtensions;
        private readonly ContentDescription _Contents;
        private readonly bool _DynamicCache;
        private bool _Parsed;

        public PartialCache(string rawValue, int positionId, ArgumentCollection arguments) :
            base(DirectiveTypes.PartialCache, arguments)
        {
            this._PositionId = positionId;
            this._Parameters =
                DirectiveHelper.CaptureDirectiveParameters(rawValue, true);
            this._CacheIdExtensions = new string[this._Parameters.Length];
            this._Contents = new ContentDescription(rawValue);
            this._DynamicCache =
                this._Contents.Parts[0].StartsWith(PartialCache.DYNAMIC_CACHE_POINTER);
        }

        public override bool Searchable => false;
        public override bool Dynamic => false;
        public override bool CanAsync => false;
        public override bool CanHoldVariable => false;

        public override void Parse()
        {
            if (this._Parsed)
                return;
            this._Parsed = true;

            // PartialCache needs to link ContentArguments of its parent.
            if (this.Parent != null)
                this.Arguments.Replace(this.Parent.Arguments);

            for (int i = 0; i < this._Parameters.Length; i++)
                this._CacheIdExtensions[i] =
                    Xeora.Web.Directives.Property.Render(this.Parent, this._Parameters[i]).Item2?.ToString();

            this.Children = new DirectiveCollection(this.Mother, this);
            this.Mother.RequestParsing(
                this._DynamicCache
                    ? this._Contents.Parts[0][PartialCache.DYNAMIC_CACHE_POINTER.Length..]
                    : this._Contents.Parts[0], 
                this.Children, 
                this.Arguments
            );
        }

        private string[] _InstanceIdAccessTree;
        private string _CacheId;
        public override bool PreRender()
        {
            if (this.Status != RenderStatus.None)
                return false;
            this.Status = RenderStatus.Rendering;
            
            this.Mother.RequestInstance(out Basics.Domain.IDomain instance);
            this._InstanceIdAccessTree = instance.IdAccessTree;
            this._CacheId =
                PartialCacheObject.CreateUniqueCacheId(this._PositionId, string.Join('_', this._CacheIdExtensions), this, ref instance);
            PartialCachePool.Current.Get(this._InstanceIdAccessTree, this._CacheId, out PartialCacheObject cacheObject);

            if (cacheObject == null)
                this.Parse();
            else
            {
                this.Children = new DirectiveCollection(this.Mother, this);
                this.Children.AddRange(cacheObject.Directives);
            }
            
            return true;
        }

        public override void PostRender()
        {
            this.Deliver(RenderStatus.Rendered, this.Result);

            if (!this._DynamicCache)
            {
                PartialCachePool.Current.AddOrUpdate(
                    this._InstanceIdAccessTree,
                    new PartialCacheObject(
                        this._CacheId,
                        new List<IDirective>(new[] { new Static(this.Result) }))
                );
                return;
            }

            List<IDirective> cachedDirectives =
                new List<IDirective>();
            foreach (IDirective directive in this.Children)
                cachedDirectives.Add(directive.Dynamic ? directive : new Static(directive.Result));
            
            PartialCachePool.Current.AddOrUpdate(
                this._InstanceIdAccessTree,
                new PartialCacheObject(this._CacheId, cachedDirectives)
            );
        }
    }
}