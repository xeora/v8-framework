﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Net;
using Xeora.Web.Basics.Configuration;

namespace Xeora.Web.Configuration
{
    public class Dss : IDss
    {
        public Dss() =>
            this.ServiceType = DssServiceTypes.BuiltIn;

        [DefaultValue(DssServiceTypes.BuiltIn)]
        [JsonProperty(PropertyName = "serviceType", DefaultValueHandling = DefaultValueHandling.Populate)]
        public DssServiceTypes ServiceType { get; private set; }

        [DefaultValue("127.0.0.1:5531")]
        [JsonProperty(PropertyName = "serviceEndPoint", DefaultValueHandling = DefaultValueHandling.Populate)]
        private string _ServiceEndPoint { get; set; }

        public IPEndPoint ServiceEndPoint
        {
            get
            {
                if (string.IsNullOrEmpty(this._ServiceEndPoint))
                    this._ServiceEndPoint = "127.0.0.1:5531";

                int colonIndex = this._ServiceEndPoint.IndexOf(':');
                if (colonIndex == -1)
                    this._ServiceEndPoint = $"{this._ServiceEndPoint}:5531";

                string serverAddress =
                    this._ServiceEndPoint.Split(':')[0];

                IPAddress serviceIp = IPAddress.Parse("127.0.0.1");
                try
                {
                    IPAddress[] ipAddresses =
                        Dns.GetHostAddresses(serverAddress);

                    if (ipAddresses.Length == 0)
                        throw new Exception("Service EndPoint is not possible to resolved");

                    serviceIp = ipAddresses[0];
                }
                catch (Exception e)
                {
                    Basics.Logging.Current
                        .Error(
                            "Dss EndPoint resolution error!", 
                            new Dictionary<string, object>
                            {
                                { "message", e.Message }
                            }
                        )
                        .Flush();
                }

                if (!int.TryParse(this._ServiceEndPoint.Split(':')[1], out int servicePort))
                    servicePort = 5531;
                
                return new IPEndPoint(serviceIp, servicePort);
            }
        }
    }
}
