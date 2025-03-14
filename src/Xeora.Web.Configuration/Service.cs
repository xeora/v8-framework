﻿using Newtonsoft.Json;
using System.ComponentModel;
using System.Net;

namespace Xeora.Web.Configuration
{
    public class Service : Basics.Configuration.IService
    {
        public Service()
        {
            this._Address = "127.0.0.1";
            this.Port = 3381;
            this.Timeout = new Timeout();
            this.Ssl = false;
            this.CertificatePassword = string.Empty;
            this.Parallelism = new Parallelism();
            this.Logging = true;
            this.LoggingFormat = Basics.Configuration.LoggingFormats.Plain;
            this.LoggingLevel = Basics.Configuration.LoggingTypes.Info;
        }

        [DefaultValue("127.0.0.1")]
        [JsonProperty(PropertyName = "address", DefaultValueHandling = DefaultValueHandling.Populate)]
        private string _Address { get; set; }

        public IPAddress Address => IPAddress.Parse(this._Address);

        [DefaultValue(3381)]
        [JsonProperty(PropertyName = "port", DefaultValueHandling = DefaultValueHandling.Populate)]
        public short Port { get; private set; }
        
        [JsonProperty(PropertyName = "timeout")]
        public Basics.Configuration.ITimeout Timeout { get; private set; }

        [DefaultValue(false)]
        [JsonProperty(PropertyName = "ssl", DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Ssl { get; private set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "certificatePassword", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string CertificatePassword { get; private set; }
        
        [JsonProperty(PropertyName = "parallelism")]
        public Basics.Configuration.IParallelism Parallelism { get; private set; }

        [DefaultValue(true)]
        [JsonProperty(PropertyName = "logging", DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Logging { get; private set; }
        
        [DefaultValue(Basics.Configuration.LoggingFormats.Plain)]
        [JsonProperty(PropertyName = "loggingFormat", DefaultValueHandling = DefaultValueHandling.Populate)]
        public Basics.Configuration.LoggingFormats LoggingFormat { get; private set; }
        
        [DefaultValue(Basics.Configuration.LoggingTypes.Info)]
        [JsonProperty(PropertyName = "loggingLevel", DefaultValueHandling = DefaultValueHandling.Populate)]
        public Basics.Configuration.LoggingTypes LoggingLevel { get; private set; }
    }
}
