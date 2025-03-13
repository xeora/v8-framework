using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Xeora.Web.Service.Workers
{
    public class Bulk
    {
        private readonly Action<List<ActionContainer>> _QueueHandler;
        
        private readonly ConcurrentDictionary<string, bool> _ActionTracker;
        private readonly List<ActionContainer> _ActionContainers;
        
        private readonly object _Lock = new();

        internal Bulk(Action<List<ActionContainer>> queueHandler)
        {
            this._QueueHandler = queueHandler;
            
            this._ActionTracker = new ConcurrentDictionary<string, bool>();
            this._ActionContainers = new List<ActionContainer>();
        }

        private void Completed(string id)
        {
            Monitor.Enter(this._Lock);
            try
            {
                this._ActionTracker.TryRemove(id, out _);
                if (!this._ActionTracker.IsEmpty) return;
                
                Monitor.Pulse(this._Lock);
            }
            finally
            {
                Monitor.Exit(this._Lock);
            }
        }

        public void Add(Action<object> startHandler, object state, ActionType actionType)
        {
            if (actionType == ActionType.Connection)
                throw new ArgumentException("Not allowed to queue the Connection action into context");

            ActionContainer actionContainer =
                new ActionContainer((_, s) => startHandler(s), state, actionType, this.Completed);
                
            this._ActionTracker.TryAdd(actionContainer.Id, true);
            this._ActionContainers.Add(actionContainer);
        }
        
        public void Process()
        {
            if (this._ActionTracker.IsEmpty) return;
            
            this._QueueHandler.Invoke(this._ActionContainers);
            
            if (this._ActionTracker.IsEmpty) return;
            
            Monitor.Enter(this._Lock);
            try
            {
                if (this._ActionTracker.IsEmpty) return;
                
                Monitor.Wait(this._Lock);
            }
            finally
            {
                Monitor.Exit(this._Lock);
            }
        }
    }
}
