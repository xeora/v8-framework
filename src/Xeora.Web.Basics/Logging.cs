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
        private readonly ConcurrentDictionary<string, ConcurrentQueue<Message>> _MessageGroups;
        private readonly object _FlushingLock = new();
        private readonly ConcurrentDictionary<string, bool> _Flushing;

        private static readonly Newtonsoft.Json.JsonSerializer JsonSerializer =
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
            this._MessageGroups = new ConcurrentDictionary<string, ConcurrentQueue<Message>>();
            this._Flushing = new ConcurrentDictionary<string, bool>();
        }

        private void Queue(Message message, string groupId)
        {
            if (string.IsNullOrEmpty(groupId)) 
                groupId = Guid.Empty.ToString();

            this._MessageGroups.AddOrUpdate(
                groupId,
                new ConcurrentQueue<Message>(new[] { message }),
                (_, queue) =>
                {
                    queue.Enqueue(message);
                    return queue;
                });
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
                if (!this._MessageGroups.TryRemove(groupId, out ConcurrentQueue<Message> messages))
                    return;

                while (messages.TryDequeue(out Message message))
                    Logging.WriteLine(message);

                if (string.CompareOrdinal(groupId, Guid.Empty.ToString()) == 0) return;
                
                string separator =
                    "".PadRight(30, '-');
                Message separatorMessage =
                    Configurations.Xeora.Service.LoggingFormat switch
                    {
                        LoggingFormats.Json => this.FormatJson(LoggingTypes.Info, separator),
                        _ => this.FormatPlain(LoggingTypes.Info, separator)
                    };
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
                System.Console.WriteLine(message.Value);
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
                    formattedMessage.Append("".PadRight(System.Console.WindowWidth, ' '));
                    continue;
                }
                formattedMessage.AppendLine(m.PadRight(System.Console.WindowWidth, ' '));
            }

            switch (message.Type)
            {
                case LoggingTypes.Warn:
                    System.Console.BackgroundColor = ConsoleColor.Yellow;
                    System.Console.ForegroundColor = ConsoleColor.Black;
                    System.Console.Write(formattedMessage);
                    System.Console.ResetColor();
                    return;
                case LoggingTypes.Error:
                    System.Console.BackgroundColor = ConsoleColor.Red;
                    System.Console.ForegroundColor = ConsoleColor.White;
                    System.Console.Write(formattedMessage);
                    System.Console.ResetColor();
                    return;
                case LoggingTypes.Trace:
                case LoggingTypes.Debug:
                case LoggingTypes.Info:
                default:
                    System.Console.Write(formattedMessage);
                    System.Console.ResetColor();
                    return;
            }
        }

        private static readonly object Lock = new();
        private static Logging _current;
        
        /// <summary>
        /// Provides active logging instance
        /// </summary>
        /// <returns>Active Logging Instance Object</returns>
        public static Logging Current
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
        /// <returns>Logging Object for chain reaction</returns>
        /// <param name="message">Message Content</param>
        /// <param name="fields">Message Details or fields to print</param>
        /// <param name="groupId">Give a unique id for the push to group all the inputs together</param>
        public Logging Trace(
            string message, 
            Dictionary<string, object> fields = null,
            string groupId = null) =>
            this.Push(LoggingTypes.Trace, message, fields, groupId);
        
        /// <summary>
        /// Push the message to the Xeora framework console as debug
        /// </summary>
        /// <returns>Logging Object for chain reaction</returns>
        /// <param name="message">Message Content</param>
        /// <param name="fields">Message Details or fields to print</param>
        /// <param name="groupId">Give a unique id for the push to group all the inputs together</param>
        public Logging Debug(
            string message, 
            Dictionary<string, object> fields = null,
            string groupId = null) =>
            this.Push(LoggingTypes.Debug, message, fields, groupId);

        /// <summary>
        /// Push the message to the Xeora framework console as information
        /// </summary>
        /// <returns>Logging Object for chain reaction</returns>
        /// <param name="message">Message Content</param>
        /// <param name="fields">Message Details or fields to print</param>
        /// <param name="groupId">Give a unique id for the push to group all the inputs together</param>
        public Logging Information(
            string message, 
            Dictionary<string, object> fields = null,
            string groupId = null) =>
            this.Push(LoggingTypes.Info, message, fields, groupId);
        
        /// <summary>
        /// Push the message to the Xeora framework console as warning
        /// </summary>
        /// <returns>Logging Object for chain reaction</returns>
        /// <param name="message">Message Content</param>
        /// <param name="fields">Message Details or fields to print</param>
        /// <param name="groupId">Give a unique id for the push to group all the inputs together</param>
        public Logging Warning(
            string message, 
            Dictionary<string, object> fields = null,
            string groupId = null) =>
            this.Push(LoggingTypes.Warn, message, fields, groupId);
        
        /// <summary>
        /// Push the message to the Xeora framework console as error
        /// </summary>
        /// <returns>Logging Object for chain reaction</returns>
        /// <param name="message">Message Content</param>
        /// <param name="fields">Message Details or fields to print</param>
        /// <param name="groupId">Give a unique id for the push to group all the inputs together</param>
        public Logging Error(
            string message, 
            Dictionary<string, object> fields = null,
            string groupId = null) =>
            this.Push(LoggingTypes.Error, message, fields, groupId);

        /// <summary>
        /// Push the message to the Xeora framework console as error and quits
        /// </summary>
        /// <param name="message">Message Content</param>
        /// <param name="fields">Message Details or fields to print</param>
        /// <param name="groupId">Give a unique id for the push to group all the inputs together</param>
        public void Fatal(
            string message,
            Dictionary<string, object> fields = null,
            string groupId = null)
        {
            this.Push(LoggingTypes.Error, message, fields, groupId)
                .Flush()
                .Wait();
            
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
        
        private Logging Push(LoggingTypes type, string message, Dictionary<string, object> fields = null, string groupId = null)
        {
            if (!Configurations.Xeora.Service.Logging) return Logging.Current;
            if (!MatchLoggingLevel(type)) return Logging.Current;

            return this.Print(
                Configurations.Xeora.Service.LoggingFormat switch
                {
                    LoggingFormats.Json => this.FormatJson(type, message, fields),
                    _ => this.FormatPlain(type, message, fields)
                },
                groupId
            );
        }

        private Message FormatPlain(LoggingTypes type, string message, Dictionary<string, object> fields = null)
        {
            string typePointer = 
                type.ToString().ToUpperInvariant().PadRight(5, ' ');

            string consoleMessage = 
                $"{DateTime.UtcNow:MM/dd/yyyy HH:mm:ss.fff} {typePointer} {message}";

            if (fields != null)
                foreach (var (key, value) in fields)
                    consoleMessage = $"{consoleMessage}, {key}={value}";

            return Message.Create(type, consoleMessage);
        }

        private Message FormatJson(LoggingTypes type, string message, Dictionary<string, object> fields = null)
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
                SerializeToJson(jsonObject);

            return Message.Create(type, consoleMessage);
        }
        
        private Logging Print(Message message, string groupId = null)
        {
            this.Queue(message, groupId);
            return this;
        }

        private static string SerializeToJson(object value)
        {
            StringWriter sW = null;
            Newtonsoft.Json.JsonTextWriter jsonWriter = null;
            try
            {
                sW = new StringWriter();
                jsonWriter = new Newtonsoft.Json.JsonTextWriter(sW);
                Logging.JsonSerializer.Serialize(jsonWriter, value);

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
        public Task Flush(string groupId = null) =>
            Task.Factory.StartNew(() => this._Flush(groupId));
    }
}
