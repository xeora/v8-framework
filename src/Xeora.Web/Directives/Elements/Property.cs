using System;
using Xeora.Web.Global;

namespace Xeora.Web.Directives.Elements
{
    public class Property : Directive
    {
        private readonly string _RawValue;

        public Property(string rawValue, ArgumentCollection arguments) :
            base(DirectiveTypes.Property, arguments)
        {
            this._RawValue = rawValue;
            this.CanAsync = true;

            if (string.IsNullOrEmpty(this._RawValue))
                return;
            
            switch (this._RawValue)
            {
                case "DomainContents":
                    this.Dynamic = false;
                    break;
                case "PageRenderDuration":
                    break;
                default:
                    switch (this._RawValue[0])
                    {
                        case PropertyConstants.CONSTANT:
                            this.Dynamic = false;
                            break;
                        case PropertyConstants.QUERY:
                        case PropertyConstants.FORM:
                        case PropertyConstants.SESSION:
                        case PropertyConstants.APPLICATION:
                        case PropertyConstants.COOKIE:
                        case PropertyConstants.DATA_FIELD:
                            break;
                        case PropertyConstants.OBJECT:
                            switch (this._RawValue[1])
                            {
                                case PropertyConstants.SESSION:
                                case PropertyConstants.APPLICATION:
                                case PropertyConstants.DATA_FIELD:
                                case PropertyConstants.DIRECTIVE:
                                    break;
                                default:
                                    this.CanAsync = false;

                                    break;
                            }

                            break;
                        default:
                            this.CanAsync = false;

                            break;
                    }
                    break;
            }
        }

        public override bool Searchable => false;
        public override bool Dynamic { get; } = true;

        public override bool CanAsync { get; }
        public override bool CanHoldVariable => false;

        public override void Parse() =>
            this.Children = new DirectiveCollection(this.Mother, this);

        public override bool PreRender()
        {
            if (this.Status != RenderStatus.None)
                return false;
            this.Status = RenderStatus.Rendering;

            this.Parse();
            
            if (string.IsNullOrEmpty(this._RawValue))
            {
                this.Deliver(RenderStatus.Rendered, string.Empty);
                return false;
            }

            Tuple<bool, object> result = 
                Directives.Property.Render(this, this._RawValue);

            if (!result.Item1) return false;
            
            if (result.Item2 != null)
                this.Children.Add(new Static(result.Item2.ToString()));

            return true;
        }

        public override void PostRender() =>
            this.Deliver(RenderStatus.Rendered, this.Result);
    }
}