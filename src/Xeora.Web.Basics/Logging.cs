using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xeora.Web.Basics.Configuration;

namespace Xeora.Web.Basics
{
    public class Logging
    {
        private readonly object _MessageLock = new();
        private readonly Dictionary<string, Queue<Message>> _MessageGroups;
        private readonly ConcurrentDictionary<string, Action<ConsoleKeyInfo>> _KeyListeners;
        private readonly object _FlushingLock = new();
        private readonly ConcurrentDictionary<string, bool> _Flushing;

        private static readonly Newtonsoft.Json.JsonSerializer _JsonSerializer =
            Newtonsoft.Json.JsonSerializer.CreateDefault();
        
        private class Message
        {
            private Message(LoggingTypes type, string value)
            {
                this.Type = type;
                this.Value = value;
            }
            
            public LoggingTypes Type { get; }
            public string Value { get; }

            public static Message Create(LoggingTypes type, string value) =>
                new(type, value);
        }
        
        private Logging()
        {
            this._MessageGroups = new Dictionary<string, Queue<Message>>();
            this._KeyListeners = new ConcurrentDictionary<string, Action<ConsoleKeyInfo>>();
            this._Flushing = new ConcurrentDictionary<string, bool>();

            ThreadPool.QueueUserWorkItem(_ => this.StartKeyListener());
        }

        private void Queue(Message message, string groupId)
        {
            if (string.IsNullOrEmpty(groupId)) 
                groupId = Guid.Empty.ToString();

            lock (this._MessageLock)
            {
                if (!this._MessageGroups.ContainsKey(groupId))
                    this._MessageGroups[groupId] = new Queue<Message>();
                
                this._MessageGroups[groupId].Enqueue(message);
            }
        }

        private void _Flush(string groupId = null)
        {
            if (string.IsNullOrEmpty(groupId))
                groupId = Guid.Empty.ToString();
            
            if (this._Flushing.ContainsKey(groupId))
                return;
            this._Flushing.TryAdd(groupId, true);

            Monitor.Enter(this._FlushingLock);
            try
            {
                Queue<Message> messages;

                lock (this._MessageLock)
                {
                    if (!this._MessageGroups.ContainsKey(groupId))
                        return;

                    messages = this._MessageGroups[groupId];

                    this._MessageGroups.Remove(groupId);
                }

                while (messages.Count > 0)
                {
                    Message message =
                        messages.Dequeue();
                    Logging.WriteLine(message);
                }

                if (string.CompareOrdinal(groupId, Guid.Empty.ToString()) == 0) return;
                
                string separator =
                    "".PadRight(30, '-');
                Message separatorMessage =
                    Message.Create(LoggingTypes.Info, $"{DateTime.UtcNow:MM/dd/yyyy HH:mm:ss.fff} ----- {separator} {separator}");
                Logging.WriteLine(separatorMessage);
            }
            finally
            {
                Monitor.Exit(this._FlushingLock);
                this._Flushing.TryRemove(groupId, out _);
            }
        }

        private static void WriteLine(Message message)
        {
            if (Configurations.Xeora.Service.LoggingFormat == LoggingFormats.Json)
            {
                Console.WriteLine(message.Value);
                return;
            }
            
            StringBuilder formattedMessage = 
                new StringBuilder();
            StringReader sR = 
                new StringReader(message.Value);
            
            while (sR.Peek() > -1)
            {
                string m = sR.ReadLine();
                if (string.IsNullOrEmpty(m))
                {
                    formattedMessage.Append("".PadRight(Console.WindowWidth, ' '));
                    continue;
                }
                formattedMessage.AppendLine(m.PadRight(Console.WindowWidth, ' '));
            }

            switch (message.Type)
            {
                case LoggingTypes.Trace:
                case LoggingTypes.Debug:
                case LoggingTypes.Info:
                    Console.Write(formattedMessage);
                    Console.ResetColor();
                    return;
                case LoggingTypes.Warn:
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write(formattedMessage);
                    Console.ResetColor();
                    return;
                case LoggingTypes.Error:
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(formattedMessage);
                    Console.ResetColor();
                    return;
            }
        }

        private void StartKeyListener()
        {
            do
            {
                ConsoleKeyInfo keyInfo;
                try
                {
                    keyInfo = Console.ReadKey(true);
                }
                catch (InvalidOperationException)
                {
                    Logging.Warning("Console inputs are not available!");

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

        private static readonly object Lock = new();
        private static Logging _current;
        private static Logging Current
        {
            get
            {
                Monitor.Enter(Logging.Lock);
                try
                {
                    return Logging._current ?? (Logging._current = new Logging());
                }
                finally
                {
                    Monitor.Exit(Logging.Lock);
                }
            }
        }
        
        /// <summary>
        /// Push the message to the Xeora framework console as trace
        /// </summary>
        /// <param name="message">Message Content</param>
        /// <param name="fields">Message Details or fields to print</param>
        /// <param name="groupId">Give a unique id for the push to group all the inputs together</param>
        public static void Trace(
            string message, 
            Dictionary<string, object> fields = null,
            string groupId = null) =>
            Logging.Push(LoggingTypes.Trace, message, fields, groupId);
        
        /// <summary>
        /// Push the message to the Xeora framework console as debug
        /// </summary>
        /// <param name="message">Message Content</param>
        /// <param name="fields">Message Details or fields to print</param>
        /// <param name="groupId">Give a unique id for the push to group all the inputs together</param>
        public static void Debug(
            string message, 
            Dictionary<string, object> fields = null,
            string groupId = null) =>
            Logging.Push(LoggingTypes.Debug, message, fields, groupId);

        /// <summary>
        /// Push the message to the Xeora framework console as information
        /// </summary>
        /// <param name="message">Message Content</param>
        /// <param name="fields">Message Details or fields to print</param>
        /// <param name="groupId">Give a unique id for the push to group all the inputs together</param>
        public static void Information(
            string message, 
            Dictionary<string, object> fields = null,
            string groupId = null) =>
            Logging.Push(LoggingTypes.Info, message, fields, groupId);
        
        /// <summary>
        /// Push the message to the Xeora framework console as warning
        /// </summary>
        /// <param name="message">Message Content</param>
        /// <param name="fields">Message Details or fields to print</param>
        /// <param name="groupId">Give a unique id for the push to group all the inputs together</param>
        public static void Warning(
            string message, 
            Dictionary<string, object> fields = null,
            string groupId = null) =>
            Logging.Push(LoggingTypes.Warn, message, fields, groupId);
        
        /// <summary>
        /// Push the message to the Xeora framework console as error
        /// </summary>
        /// <param name="message">Message Content</param>
        /// <param name="fields">Message Details or fields to print</param>
        /// <param name="groupId">Give a unique id for the push to group all the inputs together</param>
        public static void Error(
            string message, 
            Dictionary<string, object> fields = null,
            string groupId = null) =>
            Logging.Push(LoggingTypes.Error, message, fields, groupId);

        /// <summary>
        /// Push the message to the Xeora framework console as error and quits
        /// </summary>
        /// <param name="message">Message Content</param>
        /// <param name="fields">Message Details or fields to print</param>
        /// <param name="groupId">Give a unique id for the push to group all the inputs together</param>
        public static void Fatal(
            string message,
            Dictionary<string, object> fields = null,
            string groupId = null)
        {
            Logging.Push(LoggingTypes.Error, message, fields, groupId);
            Environment.Exit(99);
        }

        private static bool MatchLoggingLevel(LoggingTypes type)
        {
            switch (Configurations.Xeora.Service.LoggingLevel)
            {
                case LoggingTypes.Trace:
                case LoggingTypes.Debug when type != LoggingTypes.Trace:
                case LoggingTypes.Info when 
                    type != LoggingTypes.Trace && type != LoggingTypes.Debug:
                case LoggingTypes.Warn when 
                    type != LoggingTypes.Trace && type != LoggingTypes.Debug && type != LoggingTypes.Info:
                case LoggingTypes.Error when 
                    type != LoggingTypes.Trace && type != LoggingTypes.Debug && type != LoggingTypes.Info && type != LoggingTypes.Warn:
                    return true;
                default:
                    return false;
            }
        }
        
        private static void Push(LoggingTypes type, string message, Dictionary<string, object> fields = null, string groupId = null)
        {
            if (!Configurations.Xeora.Service.Logging) return;
            if (!Logging.MatchLoggingLevel(type)) return;

            switch (Configurations.Xeora.Service.LoggingFormat)
            {
                case LoggingFormats.Json:
                    Logging.PrintJson(type, message, fields, groupId);
                    break;
                default:
                    Logging.PrintPlain(type, message, fields, groupId);
                    break;
            }
        }
        
        private static void PrintPlain(LoggingTypes type, string message, Dictionary<string, object> fields = null, string groupId = null)
        {
            string typePointer = 
                type.ToString().ToUpperInvariant().PadRight(5, ' ');

            string consoleMessage = 
                $"{DateTime.UtcNow:MM/dd/yyyy HH:mm:ss.fff} {typePointer} {message}";

            if (fields != null)
                foreach (var (key, value) in fields)
                    consoleMessage = $"{consoleMessage}, {key}={value}";

            Logging.Current.Queue(
                Message.Create(type, consoleMessage), groupId);
        }
        
        private static void PrintJson(LoggingTypes type, string message, Dictionary<string, object> fields, string groupId = null)
        {
            Dictionary<string, object> jsonObject = new Dictionary<string, object>
            {
                ["L"] = type.ToString().ToUpperInvariant(),
                ["T"] = DateTime.UtcNow.ToString("o"),
                ["M"] = message
            };

            if (fields != null)
                foreach (var (key, value) in fields)
                {
                    if (key is "L" or "T" or "M") continue;
                    jsonObject[key] = value;
                }

            string consoleMessage =
                Logging.SerializeToJson(jsonObject);
            
            Logging.Current.Queue(
                Message.Create(type, consoleMessage), groupId);
        }


        private static string SerializeToJson(object value)
        {
            StringWriter sW = null;
            Newtonsoft.Json.JsonTextWriter jsonWriter = null;
            try
            {
                sW = new StringWriter();
                jsonWriter = new Newtonsoft.Json.JsonTextWriter(sW);
                Logging._JsonSerializer.Serialize(jsonWriter, value);

                return sW.ToString();
            }
            finally
            {
                sW?.Close();
                jsonWriter?.Close();
            }
        }

        /// <summary>
        /// Flush the caches log entries according to groupId
        /// </summary>
        /// <param name="groupId">(optional) Group Id for the flush</param>
        public static Task Flush(string groupId = null) =>
            Task.Factory.StartNew(() => Logging.Current._Flush(groupId));

        /// <summary>
        /// Register an action to Xeora framework console key listener
        /// </summary>
        /// <returns>Registration Id</returns>
        /// <param name="callback">Listener action to be invoked when a key pressed on Xeora framework console</param>
        public static string Register(Action<ConsoleKeyInfo> callback) =>
            Logging.Current.AddKeyListener(callback);

        /// <summary>
        /// Unregister an action registered with an Id previously
        /// </summary>
        /// <returns>Removal Result, <c>true</c> if removed; otherwise, <c>false</c></returns>
        /// <param name="registrationId">registration Id of action</param>
        public static bool Unregister(string registrationId) =>
            Logging.Current.RemoveKeyListener(registrationId);
    }
}
