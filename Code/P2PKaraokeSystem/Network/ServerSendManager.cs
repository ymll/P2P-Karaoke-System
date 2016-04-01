using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public class ServerSendManager : AbstractSendManager
    {
        private Int32 serverport = 12345;
        private String ServeripString = "127.0.0.1";
        public ServerSendManager()
        {
        }
        public ServerSendManager(String Serverip, Int32 setport)
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
                networkStream.Write(sendBuffer, 0, size);
                networkStream.Flush();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
            return 0;
        }

        // TODO
        public override int SendUDP(byte[] sendBuffer, int from, int size)
        {
            return 0;
        }
    }
}
