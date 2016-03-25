using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public abstract class AbstractSendManager : AbstractNetworkManager<DataSendListener>
    {
        // Return the actual number of bytes sent
        public abstract int SendTCP(byte[] sendBuffer, int from, int size);

        // Return the actual number of bytes sent
        public abstract int SendUDP(byte[] sendBuffer, int from, int size);

        protected void NotifyListeners(PacketType packetType, byte[] data)
        {
            foreach (DataSendListener listener in this.listeners[packetType])
            {
                listener.OnDataAvailable(packetType, data);
            }
        }
    }
}
