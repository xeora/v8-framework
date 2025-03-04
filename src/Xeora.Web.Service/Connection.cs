using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Xeora.Web.Service
{
    public class Connection
    {
        private readonly TcpClient _RemoteClient;
        private readonly IPEndPoint _RemoteIpEndPoint;
        private readonly X509Certificate2 _Certificate;

        public Connection(ref TcpClient remoteClient, X509Certificate2 certificate)
        {
            this._RemoteClient = remoteClient;
            this._RemoteIpEndPoint = 
                (IPEndPoint)remoteClient.Client.RemoteEndPoint;
            this._Certificate = certificate;
        }

        public void Process()
        {
            if (this.ProceedStream(out Stream remoteStream))
            {
                remoteStream.ReadTimeout = (int) Basics.Configurations.Xeora.Service.Timeout.Read;
                remoteStream.WriteTimeout = (int) Basics.Configurations.Xeora.Service.Timeout.Write;

                Net.NetworkStream streamEnclosure = 
                    new Net.NetworkStream(ref remoteStream);
                
                this.Handle(ref streamEnclosure);
            }

            remoteStream.Close();
            remoteStream.Dispose();

            this._RemoteClient.Close();
            this._RemoteClient.Dispose();
        }

        private bool ProceedStream(out Stream remoteStream)
        {
            if (Configuration.Manager.Current.Configuration.Service.Ssl)
            {
                remoteStream = 
                    new SslStream(this._RemoteClient.GetStream(), true);
                return this.Authenticate(ref remoteStream);
            }
            
            remoteStream = this._RemoteClient.GetStream();
            return true;
        }
        
        private bool Authenticate(ref Stream remoteStream)
        {
            try
            {
                ((SslStream)remoteStream).AuthenticateAsServer(this._Certificate, false, System.Security.Authentication.SslProtocols.Tls12, true);

                return true;
            }
            catch (IOException e)
            {
                Basics.Logging.Current
                    .Debug(
                        "Connection is rejected!",
                        new Dictionary<string, object>
                        {
                            { "remoteAddress", this._RemoteIpEndPoint.ToString() },
                            { "message", e.Message }
                        }
                    )
                    .Flush();

                return false;
            }
            catch (System.Exception e)
            {
                Basics.Logging.Current
                    .Error(
                        "SSL Connection FAILED!",
                        new Dictionary<string, object>
                        {
                            { "message", e.Message }
                        }
                    )
                    .Flush();

                return false;
            }
        }

        private void Handle(ref Net.NetworkStream remoteStream)
        {
            Basics.Logging.Current
                .Debug(
                    "Connection is accepted",
                    new Dictionary<string, object>
                    {
                        { "remoteAddress", this._RemoteIpEndPoint.ToString() },
                        { "ssl", Configuration.Manager.Current.Configuration.Service.Ssl }
                    }
                )
                .Flush();
            ClientState.Handle(this._RemoteIpEndPoint.Address, remoteStream);
        }
    }
}