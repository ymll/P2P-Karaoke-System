using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public class PeerServerManager
    {
        private TcpClient client;
        private byte[] buffer;

        public PeerServerManager(TcpClient client)
        {
            this.client = client;
            this.buffer = new byte[client.ReceiveBufferSize];
            NetworkStream stream = this.client.GetStream();
            stream.Read(buffer, 0, 2);

            long dataSize = BitConverter.ToInt64(buffer, 0);
            long recvedDataSize = 0;

            while (recvedDataSize < dataSize)
            {
                int recv = stream.Read(buffer, 0, buffer.Length);
                if (recv < 0)
                {
                    Trace.WriteLine("Cannot read from client");
                }
                else
                {
                    dataSize += recv;
                }
            }

            string database = System.Text.Encoding.UTF8.GetString(buffer, 0, (int)dataSize);
            database = "";
        }
    }
}
