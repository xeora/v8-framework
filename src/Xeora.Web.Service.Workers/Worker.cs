using System.Collections.Concurrent;
using System.Threading;

namespace Xeora.Web.Service.Workers
{
    internal class Worker
    {
        private readonly BlockingCollection<ActionContainer> _Queue;
        private readonly ConcurrentStack<ActionContainer> _ExternalActionStack;
        
        internal delegate void HeavyNotifiedHandler(bool heavy);
        internal event HeavyNotifiedHandler HeavyNotified;
        
        private readonly Thread _Thread;
        private ActionContainer _CurrentContainer;

        public Worker(BlockingCollection<ActionContainer> queue)
        {
            this._Queue = queue;
            this._ExternalActionStack = new ConcurrentStack<ActionContainer>();
            
            this._Thread =
                new Thread(this.Listen) {Priority = ThreadPriority.Normal, IsBackground = true};
            this._Thread.Start();
        }
        
        private void Listen()
        {
            try
            {
                while (!this._Queue.IsAddingCompleted)
                {
                    if (!this._Queue.TryTake(out ActionContainer actionContainer))
                    {
                        if (this._ExternalActionStack.TryPop(out actionContainer))
                        {
                            this.Process(actionContainer);
                            continue;
                        }
                        actionContainer = this._Queue.Take();
                    }

                    if (actionContainer.Type == ActionType.Secondary)
                    {
                        // We have a Secondary to handle
                        this.HeavyNotified?.Invoke(true);
                        this._ExternalActionStack.Push(actionContainer);
                        continue;
                    }
                    
                    this.Process(actionContainer);
                }
            }
            catch
            { /* just handle exception */ }
        }
        
        private void Process(ActionContainer actionContainer)
        {
            this._CurrentContainer = actionContainer;
            
            try
            {
                if (Common.PrintReport) 
                    this._CurrentContainer.PrintContainerDetails();
                this._CurrentContainer.Invoke();
            }
            finally
            {
                this._CurrentContainer = null;
                
                // Secondary is handled
                if (actionContainer.Type == ActionType.Secondary)
                    this.HeavyNotified?.Invoke(false);
            }
        }
        
        public void Join() =>
            this._Thread.Join(Common.JOIN_WAIT_TIMEOUT);
    }
}
