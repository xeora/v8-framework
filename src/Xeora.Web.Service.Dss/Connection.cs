using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace Xeora.Web.Service.Dss
{
    public class Connection
    {
        private readonly TcpClient _RemoteClient;
        private readonly IManager _Manager;
        private Handler _Handler;

        public Connection(ref TcpClient remoteClient, IManager manager)
        {
            this._RemoteClient = remoteClient;
            this._Manager = manager;
        }

        public void Process()
        {
            SyncStreamHandler syncStreamHandler = 
                new SyncStreamHandler(this._RemoteClient.GetStream());

            if (this.Validate(syncStreamHandler))
            {
                this._Handler = 
                    new Handler(syncStreamHandler, this._Manager);
                this.ReadSocket(syncStreamHandler);
            }

            syncStreamHandler.Dispose();
            
            this._RemoteClient.Close();
            this._RemoteClient.Dispose();
            
            Basics.Logging.Flush();
        }

        private bool Validate(SyncStreamHandler syncStreamHandler)
        {
            this._RemoteClient.ReceiveTimeout = 30000; // 30 seconds
            
            byte[] code = 
                Guid.NewGuid().ToByteArray();
            byte[] response = new byte[code.Length];
            int total = 0;
            
            try
            {
                return syncStreamHandler.Lock(syncStream =>
                {
                    syncStream.WriteByte((byte)code.Length);
                    syncStream.Write(code, 0, code.Length);

                    do
                    {
                        int bR = syncStream.Read(response, total, response.Length - total);
                        if (bR == 0) return false;
                    
                        total += bR;
                    } while (total < response.Length);

                    bool correct =
                        code.SequenceEqual(response);
                
                    syncStream.WriteByte(correct ? (byte)1 : (byte)0);

                    return correct;
                });
            }
            catch (Exception e)
            {
                // Skip SocketExceptions
                if (e is not IOException || e.InnerException is not SocketException)
                    Basics.Logging.Error(
                        "SYSTEM ERROR",
                        new Dictionary<string, object>
                        {
                            { "message", e.Message },
                            { "trace", e.ToString() }
                        }
                    );

                return false;
            }
        }
        
        private void ReadSocket(SyncStreamHandler syncStreamHandler)
        {
            // 60 seconds, client should echo to this service every 30 seconds.
            // It will have only once chance to miss, otherwise, connection will be dropped.
            this._RemoteClient.ReceiveTimeout = 60000; 
            
            byte[] head = new byte[8];
            int bR = 0;

            try
            {
                do
                {
                    // Read Head
                    bR += syncStreamHandler.ReadUnsafe(head, bR, head.Length - bR);
                    if (bR == 0) return; // if it is 0 that means, connection is zombie. Just close it.
                    if (bR != head.Length) continue;

                    this.Consume(head, syncStreamHandler);
                    
                    bR = 0;
                } while (true);
            }
            catch (Exception e)
            {
                // Skip SocketExceptions
                if (e is IOException && e.InnerException is SocketException)
                    return;

                Basics.Logging.Error(
                    "SYSTEM ERROR",
                    new Dictionary<string, object>
                    {
                        { "message", e.Message },
                        { "trace", e.ToString() }
                    }
                );
            }
        }

        private void Consume(byte[] contentHead, SyncStreamHandler syncStreamHandler)
        {
            // 8 bytes first 5 bytes are requestId, remain 3 bytes are request length. Request length can be max 15Mb
            long head = 
                BitConverter.ToInt64(contentHead, 0);
            long requestId = head >> 24;
            int contentSize = 
                (int)(head & 0xFFFFFF);

            byte[] buffer = new byte[1024];

            Stream contentStream = null;
            try
            {
                contentStream = new MemoryStream();
                do
                {
                    int readLength = buffer.Length;
                    if (contentSize < readLength)
                        readLength = contentSize;
            
                    int bR = syncStreamHandler.ReadUnsafe(buffer, 0, readLength);
                    contentStream.Write(buffer, 0, bR);

                    contentSize -= bR;
                } while (contentSize > 0);

                byte[] messageBlock = 
                    ((MemoryStream)contentStream).ToArray();
                this._Handler.ProcessAsync(requestId, messageBlock);
            }
            finally
            {
                contentStream?.Dispose();
            }
        }
    }
}
