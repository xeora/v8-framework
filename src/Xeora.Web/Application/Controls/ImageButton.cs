﻿using Xeora.Web.Basics.Domain.Control;
using Xeora.Web.Basics.Domain.Control.Definitions;
using Xeora.Web.Basics.Execution;

namespace Xeora.Web.Application.Controls
{
    public class ImageButton : Base, IImageButton
    {
        public ImageButton(Bind bind, SecurityDefinition security, string source, Updates updates, AttributeCollection attributes) :
            base(ControlTypes.ImageButton, bind, security)
        {
            this.Source = source;
            this.Updates = updates;
            this.Attributes = attributes;
        }

        public string Source { get; }
        public Updates Updates { get; }
        public AttributeCollection Attributes { get; }

        public override IBase Clone()
        {
            Bind bind = null;
            Bind?.Clone(out bind);

            SecurityDefinition security = null;
            Security?.Clone(out security);

            return new ImageButton(bind, security, this.Source, this.Updates.Clone(), this.Attributes.Clone());
        }
    }
}