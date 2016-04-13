using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public class ServerReceiveManager : AbstractReceiveManager
      {      
        // TODO: Implement TCP receiver for client
        private TcpListener server = null;
        int bufferSize = 500;
        Int32 port = 12345;
        String LocalipString = "127.0.0.1";//"192.168.0.2";// "192.168.0.5";//
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
                server = new TcpListener(IPAddress.Parse(LocalipString), port);
                server.Start();
                new Thread(ListenClientConnect).Start();              
            }
            catch
            {
              server.Stop();
            }
        }
        private void ListenClientConnect()
        {
            TcpClient newClient = null;
            while (true)
            {            
                try
                {
                    newClient = server.AcceptTcpClient();  
                    new Thread(recvThread).Start(newClient);                 
                }
                catch
                {
                }         
           }
        }
        private void recvThread(object userClient)
        {
            try
            {
                TcpClient client = (TcpClient)userClient;
                NetworkStream stream = client.GetStream();
                int i;
                byte[] destData ;//= new byte[bufferSize];
                byte[] recvBuffer = new byte[bufferSize];
                PacketType packetType;
                while ((i = stream.Read(recvBuffer, 0, recvBuffer.Length)) != 0)
                {
                 /*   Console.WriteLine("buff recv:\n");
                    for (int k = 0; k < i; k++)
                        Console.Write(recvBuffer[k]);*/

                    if (this.ParsePacket(recvBuffer, out destData, out packetType, i-payloadSize))
                    {             
                    /*    Console.WriteLine("\npacketType = ");
                        Console.WriteLine(packetType);
                        Console.WriteLine("\n");*/
                      /*  for (int k = 0; k < i-payloadSize; k++)
                            Console.Write(Convert.ToChar(destData[k]));*/
                        this.NotifyListeners(packetType, destData, ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(), ((IPEndPoint)client.Client.RemoteEndPoint).Port);
                    }

                } 
                client.Close();
            }            
            catch
            {
            }      
        }
        public void StopReceivePacket()
        {
            server.Stop();
        }
        Thread receivingThread;
        // TODO: Implement UDP receiver for client
        public override void StartReceiveUdpDatagram()
        { 
            UdpClient client = new UdpClient(12345);
            ThreadStart start = new ThreadStart(Receiver);
            receivingThread = new Thread(start);
            receivingThread.IsBackground = true;
            receivingThread.Start();
        }

        UdpClient receivingClient;

        private void InitializeReceiver()
        {
            receivingClient = new UdpClient(12345);
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
                byte[] data = receivingClient.Receive(ref endPoint);

            }
        }
    }
}
