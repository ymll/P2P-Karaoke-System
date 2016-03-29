using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public class ServerReceiveManager : AbstractReceiveManager
    {
        // TODO: Implement TCP receiver for server
        private TcpListener client = null;
        int bufferSize = 500;
        Int32 port = 12345;
        String LocalipString = "127.0.0.1";
        public ServerReceiveManager() {  }//use the above setting
        public ServerReceiveManager(String Localip, Int32 newPort, int bufSize)
        {
            this.bufferSize = bufSize;
            this.port = newPort;
            this.LocalipString = Localip;
        }
        public override void StartReceiveTcpPacket()
        {
        /*
             * //call with:
             * new ClientReceiveManager().StartReceiveTcpPacket(); 
             * //in main
             */
            try
            {
                client = new TcpListener(IPAddress.Parse(LocalipString), port);
                client.Start();
                new Thread(ListenServerConnect).Start();              
            }
            catch
            {
              client.Stop();
            }
        }
        private void ListenServerConnect()
        {
            TcpServer newServer = null;
            while (true)
            {            
                try
                {
                    newServer = server.AcceptTcpServer();  
                    new Thread(recvThread).Start(newServer);                 
                }
                catch
                {
                }         
           }
        }
        private void recvThread(object userServer)
        {
            try
            {
                TcpServer  server = (TcpServer)userServer;
                NetworkStream stream = server.GetStream();
                int i;
                byte[] destData = new byte[bufferSize];
                byte[] recvBuffer = new byte[bufferSize];
                PacketType packetType;
                while ((i = stream.Read(recvBuffer, 0, recvBuffer.Length)) != 0)
                {
                    Console.WriteLine("buff recv:\n");
                    for (int k = 0; k < i; k++)
                        Console.Write(Convert.ToChar(recvBuffer[k]));
                    if (this.ParsePacket(recvBuffer, destData, out packetType))
                    {
                        this.NotifyListeners(packetType, destData);
                    }
                } 
                server.Close(); //
            }            
            catch
            {
            }      
        }
        public void StopReceivePacket()
        {
            client.Stop();//
        }
        Thread receivingThread;
        }

        // TODO: Implement UDP receiver for server
        public override void StartReceiveUdpDatagram()
        {
            UdpServer server = new UdpServer(12345);
            ThreadStart start = new ThreadStart(Receiver);
            receivingThread = new Thread(start);
            receivingThread.IsBackground = true;
            receivingThread.Start();
        }

        UdpServer receivingServer;

        private void InitializeReceiver()
        {
            receivingServer = new UdpServer(12345);
            ThreadStart start = new ThreadStart(Receiver);
            receivingThread = new Thread(start);
            receivingThread.IsBackground = true;
            receivingThread.Start();
        }
        private void Receiver()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 12345);
            while (true)
            {
                byte[] data = receivingServer.Receive(ref endPoint);

            }

        }
    }
}
