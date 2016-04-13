using P2PKaraokeSystem.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace P2PKaraokeSystem.Network
{
    public class PeerSharingManager
    {
        private readonly NetworkModel networkModel;
        private readonly VideoDatabase videoDatabase;
        private ManualResetEventSlim isSharingEvent;
        private const int SELECT_TIMEOUT_IN_MICROSECOND = 1000000;

        public PeerSharingManager(NetworkModel networkModel, VideoDatabase videoDatabase)
        {
            this.networkModel = networkModel;
            this.isSharingEvent = new ManualResetEventSlim();
        }

        public void StartSharing()
        {
            Thread acceptThread = new Thread(() =>
            {
                TcpListener tcpListener = StartLocalServer();
                WaitForNewConnection(tcpListener);
            });
            acceptThread.Name = "Network Select";
            acceptThread.Start();
            isSharingEvent.Set();
        }

        private TcpListener StartLocalServer()
        {
            TcpListener tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
            tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            tcpListener.Start();
            this.networkModel.LocalPort = (tcpListener.LocalEndpoint as IPEndPoint).Port;

            return tcpListener;
        }

        private void WaitForNewConnection(TcpListener tcpListener)
        {
            var socketList = new ArrayList();
            for (; ; )
            {
                socketList.Add(tcpListener.Server);
                Socket.Select(socketList, null, null, SELECT_TIMEOUT_IN_MICROSECOND);
                if (socketList.Contains(tcpListener.Server))
                {
                    Socket newClientSocket = tcpListener.Server.Accept();
                    Trace.WriteLine("New client is connected");
                    ThreadPool.QueueUserWorkItem(RecvThread, newClientSocket);
                }
                Trace.WriteLine("Waiting for new connection");
            }
        }

        private void RecvThread(object state)
        {
            Socket newClientSocket = (Socket)state;
            TcpClient newClient = new TcpClient();

            newClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 1024000);
            newClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 1024000);
            newClient.Client = newClientSocket;

            new PeerServerManager(newClient);
        }

        public void ConnectRemoteDevice(string server, string port)
        {
            int portInt;
            if (int.TryParse(port, out portInt))
            {
                TcpClient localClient = new TcpClient(server, portInt);
                NetworkStream serverStream = localClient.GetStream();

                SendDatabaseToRemote(serverStream);
            }
        }

        private void SendDatabaseToRemote(NetworkStream stream)
        {
            string db = videoDatabase.SaveToText();
            byte[] dbBytes = Encoding.ASCII.GetBytes(db);
            byte[] dataSize = BitConverter.GetBytes((long)dbBytes.Length);
            stream.Write(dataSize, 0, dataSize.Length);
            stream.Write(dbBytes, 0, dbBytes.Length);
        }
    }
}
