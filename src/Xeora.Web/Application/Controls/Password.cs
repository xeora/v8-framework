﻿using Xeora.Web.Basics.Domain.Control;
using Xeora.Web.Basics.Domain.Control.Definitions;
using Xeora.Web.Basics.Execution;

namespace Xeora.Web.Application.Controls
{
    public class Password : Base, IPassword
    {
        public Password(Bind bind, SecurityDefinition security, string text, string defaultButtonId, Updates updates, AttributeCollection attributes) :
            base(ControlTypes.Password, bind, security)
        {
            this.Text = text;
            this.DefaultButtonId = defaultButtonId;
            this.Updates = updates;
            this.Attributes = attributes;
        }

        public string Text { get; }
        public string DefaultButtonId { get; }
        public Updates Updates { get; }
        public AttributeCollection Attributes { get; }

        public override IBase Clone()
        {
            Bind bind = null;
            Bind?.Clone(out bind);

            SecurityDefinition security = null;
            Security?.Clone(out security);

            return new Password(bind, security, this.Text, this.DefaultButtonId, this.Updates.Clone(), this.Attributes.Clone());
        }
    }
}