using System;
using System.Collections.Generic;
using System.Threading;

namespace Xeora.Web.Service.Workers
{
    internal class ConnectionManager
    {
        private WorkerPool _WorkerPool;
        
        internal delegate void CompletedHandler(string contextId, WorkerPool workerPool);
        internal event CompletedHandler Completed;
        
        private readonly ActionContainer _ConnectionActionContainer;
        private readonly Thread _ConnectionThread;

        public ConnectionManager(ActionContainer connectionActionContainer)
        {
            this.Id = Guid.NewGuid().ToString();
            
            this._ConnectionActionContainer = connectionActionContainer;
            this._ConnectionActionContainer.AssignConnectionId(this.Id);
            
            this._ConnectionThread =
                new Thread(this.ProcessConnection) {Priority = ThreadPriority.Normal, IsBackground = true};
        }
        
        private void ProcessConnection() =>
            this._ConnectionActionContainer.Invoke();

        public string Id { get; }

        public void Assign(ref WorkerPool workerPool) => this._WorkerPool = workerPool;

        public void Start() =>
            this._ConnectionThread.Start();
        
        private void ProcessQueue(List<ActionContainer> actionContainerList)
        {
            try
            {
                Stack<ActionContainer> secondaryActionStack = 
                    new Stack<ActionContainer>();
                foreach (ActionContainer actionContainer in actionContainerList)
                {
                    actionContainer.AssignConnectionId(this.Id);
                        
                    if (actionContainer.Type == ActionType.Secondary)
                    {
                        secondaryActionStack.Push(actionContainer);
                        continue;
                    }
                        
                    this._WorkerPool
                        .Schedule(actionContainer);
                }

                while (secondaryActionStack.TryPop(out ActionContainer queuedActionContainer))
                    this._WorkerPool.Schedule(queuedActionContainer);
            }
            catch (Exception e)
            {
                Basics.Logging.Current
                   .Error(
                       "Connection Manager Exception...",
                       new Dictionary<string, object>
                       {
                           { "message", e.Message },
                           { "trace", e.ToString() }
                       }
                   )
                   .Flush();
            }
        }

        public void Queue(List<ActionContainer> actionContainerList) => 
            this.ProcessQueue(actionContainerList);

        public void Complete() =>
            this.Completed?.Invoke(this.Id, this._WorkerPool);
    }
}
