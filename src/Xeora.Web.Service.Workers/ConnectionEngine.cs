using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Xeora.Web.Service.Workers
{
    internal class ConnectionEngine
    {
        private readonly BlockingCollection<WorkerPool> _WorkerPools;
        private readonly ConcurrentDictionary<string, ConnectionManager> _ConnectionManagerAssignment;

        private SpinWait _SpinWait;
        
        public ConnectionEngine(ushort maxConnection, ushort magnitude)
        {
            this._WorkerPools = 
                new BlockingCollection<WorkerPool>();
            for (short i = 0; i < maxConnection; i++)
                this._WorkerPools.Add(new WorkerPool(magnitude));
         
            this._ConnectionManagerAssignment = 
                new ConcurrentDictionary<string, ConnectionManager>();
            
            this._SpinWait = new SpinWait();
        }
        public bool Disposed { get; private set; }

        public void Spin(ActionContainer actionContainer)
        {
            if (this.Disposed) return;

            ConnectionManager manager =
                new ConnectionManager(actionContainer);
            WorkerPool workerPool =
                this._WorkerPools.Take();

            this._ConnectionManagerAssignment.TryAdd(manager.Id, manager);

            manager.Completed += (cId, wP) =>
            {
                this._ConnectionManagerAssignment.TryRemove(cId, out _);
                this._WorkerPools.Add(wP);
            };
            manager.Assign(ref workerPool);
            manager.Start();
        }
        
        public void QueueToConnection(string connectionId, List<ActionContainer> actionContainerList)
        {
            if (!this._ConnectionManagerAssignment.TryGetValue(connectionId, out ConnectionManager connectionManager))
                throw new KeyNotFoundException("Connection Manager not found");
                
            connectionManager.Queue(actionContainerList);
        }

        public void FinalizeConnection(string connectionId)
        {
            if (!this._ConnectionManagerAssignment.TryRemove(connectionId, out ConnectionManager connectionManager))
                return;

            connectionManager.Complete();
        }
        
        public void Complete()
        {
            this.Disposed = true;
            
            while (!this._ConnectionManagerAssignment.IsEmpty)
                this._SpinWait.SpinOnce();
            
            foreach (WorkerPool workerPool in this._WorkerPools)
                workerPool.Kill();
        }
    }
}
