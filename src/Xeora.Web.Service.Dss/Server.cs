﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xeora.Web.Basics.Configuration;
using Task = System.Threading.Tasks.Task;

namespace Xeora.Web.Service.Dss
{
    public class Server
    {
        private readonly Mutex _TerminationLock;
        private readonly string _ConfigurationPath;
        private readonly string _ConfigurationFile;
        private readonly ConcurrentDictionary<Guid, TcpClient> _Clients;
        private readonly Thread _ClientCleanupThread;
        
        private TcpListener _TcpListener;
        private readonly IManager _Manager;

        public Server(string configurationFilePath)
        {
            this._TerminationLock = new Mutex();
            this._Clients = new ConcurrentDictionary<Guid, TcpClient>();
            this._ClientCleanupThread = new Thread(() =>
            {
                Basics.Logging.Current
                    .Information("Started client cleanup thread...")
                    .Flush();

                try
                {
                    while (true)
                    {
                        Thread.Sleep(TimeSpan.FromHours(1));

                        int purged = 0;
                        foreach (Guid key in this._Clients.Keys)
                        {
                            if (!this._Clients.TryGetValue(key, out TcpClient client)) continue;
                            if (client.Connected) continue;
                            
                            this._Clients.TryRemove(key, out _);
                            purged++;
                        }

                        if (purged == 0) continue;
                        
                        Basics.Logging.Current
                            .Information($"Purged {purged} client(s)")
                            .Flush();
                    }
                }
                catch 
                { /* Just Handle Exceptions */ }
            })
            {
                IsBackground = true, 
                Priority = ThreadPriority.Lowest
            };

            // Application Domain UnHandled Exception Event Handling
            AppDomain.CurrentDomain.UnhandledException += Server.OnUnhandledExceptions;
            // !---

            // Application Domain SIGTERM Event Handling
            AppDomain.CurrentDomain.ProcessExit += (s, e) => this.OnTerminateSignal(s, null);
            // !---
            
            Console.CancelKeyPress += this.OnTerminateSignal;

            this._ConfigurationPath = System.IO.Path.GetDirectoryName(configurationFilePath);
            this._ConfigurationFile = System.IO.Path.GetFileName(configurationFilePath);
            
            this._Manager = new Internal.Manager();
        }

        public async Task<int> StartAsync()
        {
            try
            {
                Configuration.Manager.Initialize(this._ConfigurationPath, this._ConfigurationFile);
                
                Negotiator negotiator =
                    new Negotiator();
                typeof(Basics.Helpers).InvokeMember(
                    "Packet",
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.SetProperty,
                    null,
                    null,
                    new object[] { new Basics.DomainPacket("Default", negotiator) }
                );
                
                Server.PrintLogo();
                
                IPEndPoint serviceEndPoint = 
                    new IPEndPoint(
                        Configuration.Manager.Current.Configuration.Service.Address, 
                        Configuration.Manager.Current.Configuration.Service.Port
                    );
                this._TcpListener = new TcpListener(serviceEndPoint);
                this._TcpListener.Start(100);

                Basics.Logging.Current
                    .Information($"XeoraDss is started at {serviceEndPoint}")
                    .Flush();
                
                this._ClientCleanupThread.Start();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message = $"{message} ({ex.InnerException.Message})";

                Basics.Logging.Current
                    .Error(
                        "XeoraDss is FAILED!",
                        new Dictionary<string, object>
                        {
                            { "message", message }
                        }
                    )
                    .Flush();

                return 1;
            }

            await this.ListenAsync();
            this._TerminationLock.WaitOne();

            return 0;
        }

        private async Task ListenAsync()
        {
            while (true)
            {
                try
                {
                    TcpClient remoteClient =
                        await this._TcpListener.AcceptTcpClientAsync();
                    
                    this._Clients.TryAdd(Guid.NewGuid(), remoteClient);
                    
                    ThreadPool.QueueUserWorkItem(
                        c => ((Connection) c)?.Process(),
                        new Connection(ref remoteClient, this._Manager)
                    );
                }
                catch (InvalidOperationException)
                {
                    return;
                }
                catch (SocketException)
                { /* Just Handle Exception */ }
                catch (Exception e)
                {
                    Basics.Logging.Current
                        .Debug(
                            "Connection isn't established",
                            new Dictionary<string, object>
                            {
                                { "message", e.Message }
                            }
                        )
                        .Flush();
                }
            }
        }

        private static void PrintLogo()
        {
            switch (Basics.Configurations.Xeora.Service.LoggingFormat)
            {
                case LoggingFormats.Json:
                    Basics.Logging.Current
                        .Information($"Data Structure Storage Service, v{Server.GetVersionText()}")
                        .Flush();
                    break;
                default:
                    Console.WriteLine();
                    Console.WriteLine("____  ____                               ");
                    Console.WriteLine("|_  _||_  _|                              ");
                    Console.WriteLine("  \\ \\  / /  .---.   .--.   _ .--.  ,--.   ");
                    Console.WriteLine("   > `' <  / /__\\\\/ .'`\\ \\[ `/'`\\]`'_\\ :  ");
                    Console.WriteLine(" _/ /'`\\ \\_| \\__.,| \\__. | | |    // | |, ");
                    Console.WriteLine("|____||____|'.__.' '.__.' [___]   \\'-;__/ ");

                    Console.WriteLine();
                    Console.WriteLine($"Data Structure Storage Service, v{Server.GetVersionText()}");
                    Console.WriteLine();

                    break;
            }
        }

        private static string GetVersionText()
        {
            Version vI = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{vI.Major}.{vI.Minor}.{vI.Build}";
        }

        private static void OnUnhandledExceptions(object source, UnhandledExceptionEventArgs args)
        {
            if (args?.ExceptionObject != null)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("----------- !!! --- Unhandled Exception --- !!! ------------");
                Console.WriteLine("------------------------------------------------------------");
                if (args.ExceptionObject is Exception exception)
                    Console.WriteLine(exception.ToString());
                else
                    Console.WriteLine(args.ExceptionObject.ToString());
                Console.WriteLine("------------------------------------------------------------");
                Console.WriteLine();
                Console.WriteLine();
            }

            Environment.Exit(99);
        }

        private bool _Terminating;
        private void OnTerminateSignal(object source, ConsoleCancelEventArgs args)
        {
            if (this._Terminating)
                return;
            this._Terminating = true;

            this._TerminationLock.WaitOne();
            try
            {
                if (args != null) args.Cancel = true;

                Basics.Logging.Current
                    .Information("Terminating XeoraDss...")
                    .Flush();

                this._TcpListener?.Stop();

                this._ClientCleanupThread.Interrupt();

                // Kill all connections (if any applicable)
                Basics.Logging.Current
                    .Information("Killing connected clients...")
                    .Flush();
                
                foreach (Guid key in this._Clients.Keys)
                {
                    this._Clients.TryRemove(key, out TcpClient client);
                    client?.Dispose();
                }
            }
            finally {
                this._TerminationLock.ReleaseMutex();
            }
        }
    }
}
