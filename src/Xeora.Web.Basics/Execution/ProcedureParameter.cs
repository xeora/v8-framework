using System.Collections.Generic;

namespace Xeora.Web.Basics.Execution
{
    public class ProcedureParameter
    {
        private readonly Dictionary<char, bool> _SupportedOperators =
            new() 
            {
                { '^', true }, // Query
                { '~', true }, // Form
                { '-', true }, // Session
                { '&', true }, // Application
                { '+', true }, // Cookie
                { '=', true }, // Constant
                { '#', true }, // Data Field
                { '*', true } // Wildcard
            };

        public ProcedureParameter(string parameter)
        {
            this.Query = string.Empty;
            this.Key = string.Empty;
            this.Value = null;

            if (string.IsNullOrEmpty(parameter)) return;
            
            this.Query = parameter;
            this.Key = parameter;

            if (!this._SupportedOperators.ContainsKey(this.Key[0]))
                return;

            if (this.Key[0] != '#')
                this.Key = this.Key.Substring(1);
            else
            {
                do
                {
                    if (this.Key[0] != '#')
                        break;

                    this.Key = this.Key.Substring(1);
                } while (this.Key.Length > 0);
            }
        }

        /// <summary>
        /// Gets the key of the parameter with operator
        /// </summary>
        /// <value>Parameter query</value>
        public string Query { get; }

        /// <summary>
        /// Gets the key of the parameter without operator
        /// </summary>
        /// <value>Parameter key</value>
        public string Key { get; }

        /// <summary>
        /// Gets or sets the value of the parameter
        /// </summary>
        /// <value>Parameter value</value>
        public object Value { get; set; }
    }
}
