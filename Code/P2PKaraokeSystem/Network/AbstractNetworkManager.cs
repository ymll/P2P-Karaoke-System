using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        private T FromByteArray<T>(byte[] rawValue)
        {
            GCHandle handle = GCHandle.Alloc(rawValue, GCHandleType.Pinned);
            T structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return structure;
        }

        private byte[] ToByteArray(object value, int maxLength)
        {
            int rawsize = Marshal.SizeOf(value);
            byte[] rawdata = new byte[rawsize];
            GCHandle handle =
                GCHandle.Alloc(rawdata,
                GCHandleType.Pinned);
            Marshal.StructureToPtr(value,
                handle.AddrOfPinnedObject(),
                false);
            handle.Free();
            if (maxLength < rawdata.Length)
            {
                byte[] temp = new byte[maxLength];
                Array.Copy(rawdata, temp, maxLength);
                return temp;
            }
            else
            {
                return rawdata;
            }
        }
        /**
         * Add payload to data. Return false if any error.
         */
        public bool AddPayload(out byte[] sendBuffer, byte[] data, PacketType packetType)
        {
            int payloadSize;

            byte[] temtype = ToByteArray(packetType,payloadSize);
            byte[] temret = new byte[temtype.Length + data.Length];
            System.Buffer.BlockCopy(temtype, 0, temret, 0, temtype.Length);
            System.Buffer.BlockCopy(data, 0, temret, temtype.Length, data.Length);
            sendBuffer = temret;
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
