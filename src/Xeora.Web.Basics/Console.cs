using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Xeora.Web.Basics
{
    public class Console
    {
        private readonly ConcurrentDictionary<string, Action<ConsoleKeyInfo>> _KeyListeners;
        
        private Console()
        {
            this._KeyListeners = new ConcurrentDictionary<string, Action<ConsoleKeyInfo>>();
            ThreadPool.QueueUserWorkItem(_ => this.StartKeyListener());
        }
        
        private static readonly object Lock = new();
        private static Console _current;
        private static Console Current
        {
            get
            {
                Monitor.Enter(Console.Lock);
                try
                {
                    return Console._current ?? (Console._current = new Console());
                }
                finally
                {
                    Monitor.Exit(Console.Lock);
                }
            }
        }
        
        private void StartKeyListener()
        {
            do
            {
                ConsoleKeyInfo keyInfo;
                try
                {
                    keyInfo = System.Console.ReadKey(true);
                }
                catch (InvalidOperationException)
                {
                    Logging.Current
                        .Warning("Console inputs are not available!")
                        .Flush();

                    return;
                }

                IEnumerator<KeyValuePair<string, Action<ConsoleKeyInfo>>> enumerator =
                    this._KeyListeners.GetEnumerator();

                try
                {
                    while (enumerator.MoveNext())
                    {
                        Action<ConsoleKeyInfo> action = 
                            enumerator.Current.Value;

                        ThreadPool.QueueUserWorkItem(state => ((Action<ConsoleKeyInfo>)state)?.Invoke(keyInfo), action);
                    }
                }
                finally
                {
                    enumerator.Dispose();
                }
            } while (true);
        }

        private string AddKeyListener(Action<ConsoleKeyInfo> callback)
        {
            if (callback == null)
                return Guid.Empty.ToString();

            string registrationId = 
                Guid.NewGuid().ToString();
            this._KeyListeners.TryAdd(registrationId, callback);
            
            return registrationId;
        }

        private bool RemoveKeyListener(string callbackId) =>
            !string.IsNullOrEmpty(callbackId) && this._KeyListeners.TryRemove(callbackId, out _);
        
        /// <summary>
        /// Register an action to Xeora framework console key listener
        /// </summary>
        /// <returns>Registration Id</returns>
        /// <param name="callback">Listener action to be invoked when a key pressed on Xeora framework console</param>
        public static string Register(Action<ConsoleKeyInfo> callback) =>
            Console.Current.AddKeyListener(callback);

        /// <summary>
        /// Unregister an action registered with an Id previously
        /// </summary>
        /// <returns>Removal Result, <c>true</c> if removed; otherwise, <c>false</c></returns>
        /// <param name="registrationId">registration Id of action</param>
        public static bool Unregister(string registrationId) =>
            Console.Current.RemoveKeyListener(registrationId);
    }
}
