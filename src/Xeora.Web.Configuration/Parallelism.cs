using Newtonsoft.Json;
using System.ComponentModel;

namespace Xeora.Web.Configuration
{
    public class Parallelism : Basics.Configuration.IParallelism
    {
        private ushort _MaxConnection;
        // Defines Thread Quantity For Each Connection 
        private ushort _Magnitude;
        private readonly int _AvailableProcessors = System.Environment.ProcessorCount * 2;
        
        private const ushort DEFAULT_MAX_CONNECTION = 128;

        [DefaultValue(128)]
        [JsonProperty(PropertyName = "maxConnection", DefaultValueHandling = DefaultValueHandling.Populate)]
        public ushort MaxConnection {
            get
            {
                if (this._MaxConnection == 0)
                    this._MaxConnection = Parallelism.DEFAULT_MAX_CONNECTION;
                return this._MaxConnection;
            }
            private set => this._MaxConnection = value;
        }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "magnitude", DefaultValueHandling = DefaultValueHandling.Populate)]
        public ushort Magnitude
        {
            get
            {
                if (this._Magnitude == 0)
                    this._Magnitude = (ushort)System.Math.Ceiling((double)this._AvailableProcessors / 4);
                if (this._Magnitude > this._AvailableProcessors)
                    this._Magnitude = (ushort)this._AvailableProcessors;
                return this._Magnitude;
            }
            private set => this._Magnitude = value;
        }
    }
}
