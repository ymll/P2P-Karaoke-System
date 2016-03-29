using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public class ClientSendManager : AbstractSendManager
    {
	private Int32 serverport = 12345;
        private String ServeripString = "127.0.0.1";
        public ClientSendManager()
        {
        }
        public ClientSendManager(String Serverip, Int32 setport)
        {
            this.serverport = setport;
            this.ServeripString = Serverip;
        }
        // TODO
        public override int SendTCP(byte[] sendBuffer, int from, int size)
        {  
            try
            {          
                TcpClient client = new TcpClient();
                client.Connect(ServeripString, serverport);
                NetworkStream networkStream = client.GetStream();
                networkStream.Write(sendBuffer, 0, sendBuffer.Length);
                networkStream.Flush();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
            return 0;
        }
       // const string broadcastipString = "255.255.255.255";
        // TODO
        public override int SendUDP(byte[] sendBuffer, int from, int size)
        {   
			UdpClient udpclient = new UdpClient(ServeripString, serverport);
            udpclient.Send(sendBuffer, size);
            return 0;
        }
		/*
		public override int SendUDP(byte[] sendBuffer, int from, int size)
        {   
			sendingClient = new UdpClient(broadcastipString, serverport);
            sendingClient.EnableBroadcast = true;
            sendingClient.Send(sendBuffer, size);
            return 0;
        }
		*/
    }
}
