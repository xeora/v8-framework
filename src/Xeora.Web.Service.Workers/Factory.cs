using System;
using System.Threading;

// The worker control is developed to skip the bottleneck of .NET async Task operation.
// Async task operations are based on scheduling, but they are too slow for web requests,
// and it ends with slow response time and latency on service. That's why, "Workers"
// implementation is providing agile and more responsive web service for Xeora.
namespace Xeora.Web.Service.Workers
{
    public class Factory
    {
        private const ushort MAX_WORKER_THREADS = ushort.MaxValue;
        
        private readonly ConnectionEngine _ConnectionEngine;

        private Factory(ushort maxConnection, ushort magnitude)
        {
            Factory.WorkerThreads = 
                maxConnection * magnitude;

            if (Factory.WorkerThreads > Factory.MAX_WORKER_THREADS)
            {
                Factory.WorkerThreads = Factory.MAX_WORKER_THREADS;
                magnitude = (ushort)(Factory.MAX_WORKER_THREADS / maxConnection);
            }
            
            this._ConnectionEngine = 
                new ConnectionEngine(maxConnection, magnitude);
            
            Basics.Console.Register(keyInfo => {
                if ((keyInfo.Modifiers & ConsoleModifiers.Control) == 0 || keyInfo.Key != ConsoleKey.D)
                    return;

                if (this._ConnectionEngine.Disposed)
                {
                    Basics.Logging.Current
                        .Information("Worker Factory has been already requested to be killed")
                        .Flush();
                    return;
                }

                Common.ToggleReporting();
            });
        }
        
        private void _Kill()
        {
            if (Factory._current == null) return;
            
            Basics.Logging.Current
                .Information("Worker Factory is draining...")
                .Flush();

            this._ConnectionEngine.Complete();
            
            Factory._current = null;
            
            Basics.Logging.Current
                .Information("Worker Factory is killed!")
                .Flush();
        }

        private static readonly object Lock = new();
        private static Factory _current;

        public static void Init(ushort maxConnection, ushort magnitude)
        {
            Monitor.Enter(Factory.Lock);
            try
            {
                Factory._current ??= new Factory(maxConnection, magnitude);
            }
            finally
            {
                Monitor.Exit(Factory.Lock);
            }
        }
        
        public static int WorkerThreads { get; private set; }
        
        public static void Spin(Action<string, object> action, object state) =>
            Factory._current._ConnectionEngine.Spin(new ActionContainer(action, state, ActionType.Connection));

        public static Bulk CreateBulkForConnection(string connectionId) =>
            new(actionContainerList => Factory._current._ConnectionEngine.QueueToConnection(connectionId, actionContainerList));

        public static void FinalizeConnection(string connectionId) =>
            Factory._current._ConnectionEngine.FinalizeConnection(connectionId);   
        
        public static void Kill() =>
            Factory._current?._Kill();
    }
}
