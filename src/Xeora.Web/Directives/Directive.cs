﻿using System;
using System.Collections.Generic;
using Xeora.Web.Global;

namespace Xeora.Web.Directives
{
    public abstract class Directive : IDirective
    {
        protected Directive(DirectiveTypes type, ArgumentCollection arguments)
        {
            this.UniqueId = Guid.NewGuid().ToString();

            this.Mother = null;
            this.Parent = null;
            this.UpdateBlockIds =
                new List<string>();
            this.Type = type;
            this.Arguments = arguments ?? new ArgumentCollection();

            this.Scheduler =
                new DirectiveScheduler(
                    uniqueId =>
                    {
                        this.Mother.Pool.GetByUniqueId(uniqueId, out IDirective directive);
                        directive?.Render();
                    }
                );

            this.Result = string.Empty;
        }

        public string UniqueId { get; }

        public IMother Mother { get; set; }
        public IDirective Parent { get; set; }
        public DirectiveCollection Children { get; set; }
        public string TemplateTree { get; set; }
        public List<string> UpdateBlockIds { get; }

        public DirectiveTypes Type { get; }
        public ArgumentCollection Arguments { get; }

        public DirectiveScheduler Scheduler { get; }

        public abstract bool Searchable { get; }
        public abstract bool CanAsync { get; }
        public abstract bool CanHoldVariable { get; }
        public bool HasInlineError { get; set; }
        public RenderStatus Status { get; protected set; }

        public abstract void Parse();
        public abstract bool PreRender();
        public void Render()
        {
            if (!this.PreRender())
                return;
            
            this.Children.Render();
            this.PostRender();
        }
        public abstract void PostRender();

        public void Deliver(RenderStatus status, string result)
        {
            this.Result = result;
            this.Status = status;
        }
        public string Result { get; private set; }
    }
}