using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Xeora.Web.Service.Workers
{
    internal class WorkerPool
    {
        private readonly List<Worker> _Workers;
        private readonly BlockingCollection<ActionContainer> _Queue;
        private long _HeavyCount;

        public WorkerPool(ushort magnitude)
        {
            this._Workers = new List<Worker>();
            this._Queue = new BlockingCollection<ActionContainer>();
            
            for (ushort j = 0; j < magnitude; j++)
            {
                Worker worker = new Worker(this._Queue);
                worker.HeavyNotified += heavy =>
                {
                    if (heavy)
                        Interlocked.Increment(ref _HeavyCount);
                    else
                        Interlocked.Decrement(ref _HeavyCount);
                };
                
                this._Workers.Add(worker);
            }
        }

        public bool Available => Interlocked.Read(ref this._HeavyCount) < this._Workers.Count;
        
        public void Schedule(ActionContainer actionContainer)
        {
            if (this.Available)
            {
                this._Queue.Add(actionContainer);
                return;
            }
            
            actionContainer.Invoke();
        }

        public void Kill()
        {
            this._Queue.CompleteAdding();
            
            foreach (Worker worker in this._Workers)
                worker.Join();
            
            this._Queue.Dispose();
        }
    }
}
