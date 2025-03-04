using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xeora.Web.Basics.Configuration;
using Xeora.Web.Service.Dss;

namespace Xeora.CLI.Basics
{
    public class Dss : ICommand
    {
        private IPAddress _IpAddress;
        private short _Port;
        private bool _Quite;
        private LoggingFormats _LoggingFormats;
        private LoggingTypes _LoggingLevel;
        
        private static readonly Newtonsoft.Json.JsonSerializer _JsonSerializer =
            Newtonsoft.Json.JsonSerializer.CreateDefault();

        public Dss()
        {
            this._IpAddress = IPAddress.Parse("127.0.0.1");
            this._Port = 5531;
            this._Quite = false;
            this._LoggingFormats = LoggingFormats.Plain;
            this._LoggingLevel = LoggingTypes.Info;
        }

        public void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("xeora dss OPTIONS");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("   -h, --help                  print this screen");
            Console.WriteLine("   -i, --ip IPADDRESS          ip address to listen (Default: 127.0.0.1)");
            Console.WriteLine("   -p, --port PORTNUMBER       port number to listen (Default: 5531)");
            Console.WriteLine("   -q, --quite                 deactivate logging");
            Console.WriteLine("   -f, --format [plain|json]   logging format (Default: plain)");
            Console.WriteLine("   -l, --level                 logging format (Default: info)");
            Console.WriteLine("             available values: debug|info|warn|error");
            Console.WriteLine();
        }

        private int SetArguments(IReadOnlyList<string> args)
        {
            IPAddress ipAddress = null;
            short port = 0;
            bool quite = false;
            LoggingFormats format = LoggingFormats.Plain;
            LoggingTypes level = LoggingTypes.Info;
            
            for (int aC = 0; aC < args.Count; aC++)
            {
                switch (args[aC])
                {
                    case "-h":
                    case "--help":
                        this.PrintUsage();
                        return -1;
                    case "-i":
                    case "--ip":
                        if (!Common.CheckArgument(args, aC))
                        {
                            this.PrintUsage();
                            Console.WriteLine("ip address should be specified");
                            Console.WriteLine();
                            return 2;
                        }
                        
                        if (!IPAddress.TryParse(args[aC+1], out ipAddress))
                        {
                            this.PrintUsage();
                            Console.WriteLine("ip address is not in a correct format");
                            Console.WriteLine();
                            return 2;
                        }
                        aC++;

                        break;
                    case "-p":
                    case "--port":
                        if (!Common.CheckArgument(args, aC))
                        {
                            this.PrintUsage();
                            Console.WriteLine("port should be specified");
                            Console.WriteLine();
                            return 2;
                        }
                        
                        if (!short.TryParse(args[aC + 1], out port) || port == 0)
                        {
                            this.PrintUsage();
                            Console.WriteLine("port is not a number or not in a correct range");
                            Console.WriteLine();
                            return 2;
                        }
                        aC++;

                        break;
                    case "-q":
                    case "--quite":
                        quite = true;
                        aC++;

                        break;
                    case "-f":
                    case "--format":
                        if (!Common.CheckArgument(args, aC))
                        {
                            this.PrintUsage();
                            Console.WriteLine("format should be specified");
                            Console.WriteLine();
                            return 2;
                        }
                        
                        if (!Enum.TryParse(args[aC+1], true, out format))
                        {
                            this.PrintUsage();
                            Console.WriteLine("unrecognizable format. available values: plain|json");
                            Console.WriteLine();
                            return 2;
                        }
                        aC++;

                        break;
                    case "-l":
                    case "--level":
                        if (!Common.CheckArgument(args, aC))
                        {
                            this.PrintUsage();
                            Console.WriteLine("level should be specified");
                            Console.WriteLine();
                            return 2;
                        }
                        
                        if (!Enum.TryParse(args[aC+1], true, out level))
                        {
                            this.PrintUsage();
                            Console.WriteLine("unrecognizable level. available values: debug|info|warn|error");
                            Console.WriteLine();
                            return 2;
                        }
                        aC++;

                        break;
                    default:
                        if (aC + 1 < args.Count)
                        {
                            this.PrintUsage();
                            Console.WriteLine("unrecognizable argument");
                            Console.WriteLine();
                            return 2;
                        }
                        
                        break;
                }
            }

            if (ipAddress != null)
                this._IpAddress = ipAddress;
            if (port > 0)
                this._Port = port;

            this._Quite = quite;
            this._LoggingFormats = format;
            this._LoggingLevel = level;
            
            return 0;
        }

        public async Task<int> Execute(IReadOnlyList<string> args)
        {
            int argumentsResult =
                this.SetArguments(args);
            if (argumentsResult != 0) return argumentsResult;
            
            try
            {
                Server server = 
                    new Server(this.CreateConfigurationFile());
                return await server.StartAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"XeoraDss execution problem: {e.Message}");
                return 1;
            }
        }
        
        private string CreateConfigurationFile()
        {
            Dictionary<string, object> configurationContent = 
                new Dictionary<string, object>
                {
                    ["service"] = new Dictionary<string, object>
                    {
                        { "address", this._IpAddress.ToString() },
                        { "port", this._Port },
                        { "logging", !this._Quite },
                        { "loggingFormat", this._LoggingFormats.ToString().ToLowerInvariant() },
                        { "loggingLevel", this._LoggingLevel.ToString().ToLowerInvariant() }
                    }
                };

            string filePath = 
                Path.GetTempFileName();
            
            StreamWriter sW = null;
            Newtonsoft.Json.JsonTextWriter jsonWriter = null;
            try
            {
                sW = new StreamWriter(filePath);
                jsonWriter = new Newtonsoft.Json.JsonTextWriter(sW);
                Dss._JsonSerializer.Serialize(jsonWriter, configurationContent);
            }
            finally
            {
                sW?.Close();
                jsonWriter?.Close();
            }
            
            return filePath;
        }
    }
}
