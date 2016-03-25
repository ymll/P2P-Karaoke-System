using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public class ClientReceiveManager : AbstractReceiveManager
    {
        // TODO: Implement TCP receiver for client
        public override void StartReceiveTcpPacket()
        {
            /*

            PacketType packetType;
            byte[] destData;
            byte[] recvBuffer;

            // recv(recvBuffer)

            if (this.ParsePacket(recvBuffer, destData, out packetType))
            {
                this.NotifyReceiveListener(packetType, destData);
            }
            
             */
        }

        // TODO: Implement UDP receiver for client
        public override void StartReceiveUdpDatagram()
        {

        }
    }
}
