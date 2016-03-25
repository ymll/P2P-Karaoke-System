using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public class AbstractNetworkManager<T>
    {
        protected Dictionary<PacketType, List<T>> listeners;

        public AbstractNetworkManager()
        {
            this.listeners = new Dictionary<PacketType, List<T>>();
            foreach (PacketType packetType in Enum.GetValues(typeof(PacketType)))
            {
                this.listeners.Add(packetType, new List<T>());
            }
        }

        public void RegisterListener(PacketType packetType, T listener)
        {
            if (listener != null)
            {
                listeners[packetType].Add(listener);
            }
        }

        /**
         * Add payload to data. Return false if any error.
         */
        public bool AddPayload(byte[] sendBuffer, byte[] data, PacketType packetType)
        {
            // TODO: Define packet format
            return true;
        }

        /**
         * Check the payload of packet. Return false if any error.
         */
        public bool ParsePacket(byte[] recvBuffer, byte[] destData, out PacketType packetType)
        {
            // TODO: Define packet format
            packetType = PacketType.SEARCH_QUERY;
            return true;
        }
    }
}
