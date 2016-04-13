using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public abstract class AbstractReceiveManager : AbstractNetworkManager<DataReceiveListener>
    {
        public abstract void StartReceiveTcpPacket();

        public abstract void StartReceiveUdpDatagram();

        protected void NotifyListeners(PacketType packetType, byte[] destData, String ipAddress, Int32 portNo)
        {
            foreach (DataReceiveListener listener in this.listeners[packetType])
            {
                listener.OnDataReceived(packetType, destData, ipAddress, portNo);
            }
        }
    }
}
