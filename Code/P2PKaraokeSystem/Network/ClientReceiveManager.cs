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
    public class ClientReceiveManager : AbstractReceiveManager
    {      
        // TODO: Implement TCP receiver for client
        private TcpListener server = null;
        int bufferSize;
        Int32 port = 12345;
        String LocalipString = "127.0.0.1";
		public void setInternetInfo(Int32 setport, String localip){
			port = setport;
			LocalipString = localip;
		}
		/*
		//call with:
		private void th() { 
            new ClientReceiveManager().StartReceiveTcpPacket();        
        }     
		main(){
			new Thread(th).Start();
		}
		*/
        public override void StartReceiveTcpPacket()
        {    
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
				catch(Exception e)
				{
					Console.WriteLine("Error..... " + e.StackTrace);
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
                byte[] destData = new byte[bufferSize];
                byte[] recvBuffer = new byte[bufferSize];
                PacketType packetType;
                while ((i = stream.Read(recvBuffer, 0, recvBuffer.Length)) != 0)
                {
                    Console.WriteLine(recvBuffer);
                    if (this.ParsePacket(recvBuffer, destData, out packetType))
                    {
                        this.NotifyListeners(packetType, destData);
                    }
                } 
                client.Close();
            }            
            catch(Exception e)
            {
				Console.WriteLine("Error..... " + e.StackTrace);
            }      
        }
        public void StopReceiveTcpPacket()
        {
            server.Stop();
        }

        // TODO: Implement UDP receiver for client
        public override void StartReceiveUdpDatagram()
        { 

        }     
    }
}
