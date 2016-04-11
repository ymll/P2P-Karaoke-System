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
        public static List<String> receiverAddressList;
        public static List<Int32> receiverPortList;

        public ServerSendManager()
        {
            receiverAddressList = new List<String>();
            receiverPortList = new List<Int32>();
        }
        public void NewReceiver(string ipAddress, Int32 portNum)
        {
            receiverAddressList.Add(ipAddress);
            receiverPortList.Add(portNum);
        }
        // TODO
        public override int SendTCP(byte[] sendBuffer, int from, int size)
        {
            try
            {          
                for (int i = 0; i < receiverAddressList.Count; i++)
                {
                    TcpClient client = new TcpClient();
                    client.Connect(receiverAddressList[i], receiverPortList[i]);
                    NetworkStream networkStream = client.GetStream();
                    networkStream.Write(sendBuffer, from, size);
                    networkStream.Flush();
                    client.Close();
                    Console.WriteLine("return form send");
                }
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
