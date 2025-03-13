using System;
using System.Collections.Generic;
using System.Text;

namespace Xeora.Web.Service.Workers
{
    internal class ActionContainer
    {
        private readonly Action<string> _CompletionHandler;
        
        public ActionContainer(Action<string, object> action, object state, ActionType type, Action<string> completionHandler = null)
        {
            this.Id = Guid.NewGuid().ToString();
            
            this.Action = action;
            this.State = state;
            this.Type = type;
            
            this._CompletionHandler = completionHandler;
        }

        public string Id { get; }
        
        private Action<string, object> Action { get; }
        private object State { get; }

        public string ConnectionId { get; private set; }
        public ActionType Type { get; }

        public void AssignConnectionId(string connectionId) =>
            this.ConnectionId = connectionId;
        
        public void Invoke()
        {
            try
            {
                this.Action.Invoke(this.ConnectionId, this.State);
            }
            catch (Exception e)
            {
                Basics.Logging.Current
                    .Error(
                        "Action Container Exception...",
                        new Dictionary<string, object>
                        {
                            { "message", e.Message },
                            { "trace", e.ToString() }
                        }
                    )
                    .Flush();
            }
            finally
            {
                this._CompletionHandler?.Invoke(this.Id);
            }
        }

        public void PrintContainerDetails()
        {
            if (this.State == null)
                return;

            try
            {
                StringBuilder builder = 
                    new StringBuilder();

                string typeResult = 
                    this.State.GetType().GetProperty("Type", typeof(Basics.Domain.Control.ControlTypes))?.GetMethod?
                        .Invoke(this.State, null)?.ToString();
                if (!string.IsNullOrEmpty(typeResult)) builder.Append(typeResult);
                
                string nameResult = 
                    this.State.GetType().GetInterface("INameable")?.GetProperty("DirectiveId")?.GetMethod?
                        .Invoke(this.State, null)?.ToString();
                if (!string.IsNullOrEmpty(nameResult)) 
                    builder.AppendFormat("{0}{1}", builder.Length > 0 ? "\n" : string.Empty, nameResult);

                object arguments =
                    this.State.GetType().GetProperty("Arguments")?.GetValue(this.State);
                string argumentsResult =
                    arguments?.GetType().GetMethod("ToString")?.Invoke(arguments, null)?.ToString();
                if (!string.IsNullOrEmpty(argumentsResult)) 
                    builder.AppendFormat("{0}{1}", builder.Length > 0 ? "\n" : string.Empty, argumentsResult);

                if (builder.Length == 0) return;

                Basics.Logging.Current
                    .Information(
                        "Action Container Report",
                        new Dictionary<string, object>
                        {
                            { "id", this.ConnectionId },
                            { "type", this.Type.ToString() },
                            { "summary", builder.ToString() }
                        }
                    )
                    .Flush();
            }
            catch (Exception e)
            {
                Basics.Logging.Current
                   .Error(
                       "Action Container Details Report Exception...",
                       new Dictionary<string, object>
                       {
                           { "message", e.Message },
                           { "trace", e.ToString() }
                       }
                   )
                   .Flush();
            }
        }
    }
}
