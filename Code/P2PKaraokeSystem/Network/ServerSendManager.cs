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
        public static List<String> receiverAddressList = new List<String>();
        public static List<int> receiverPortList = new List<int>();
        public static int numOfConnecter = 0;

        public ServerSendManager()
        {
        }

        public void NewReceiver(string ipAddress, int portNum)
        {
            if (numOfConnecter>0)
            {
                for (int i = 0; i < numOfConnecter; i++)
                {
                    if (ipAddress.Equals(receiverAddressList[i]) && portNum.Equals(receiverPortList[i]))
                    {
                        return;
                    }
                }
            }
            receiverAddressList.Add(ipAddress);
            receiverPortList.Add(portNum);
            numOfConnecter++;
        }
        // TODO
        public override int SendTCP(byte[] sendBuffer, int from, int size)
        {
            try
            {
                for (int i = 0; i < numOfConnecter; i++)
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