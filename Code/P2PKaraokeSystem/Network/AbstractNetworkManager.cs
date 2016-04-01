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

        /**
         * Add payload to data. Return false if any error.
         */
        protected int payloadSize = 2;
        public bool AddPayload(out byte[] sendBuffer, byte[] data, PacketType packetType)
        {
            
            byte[] temtype;
            try
            {
                temtype = BitConverter.GetBytes((Int32)packetType).Take(payloadSize).ToArray(); 
                byte[] temret = new byte[temtype.Length + data.Length];
                System.Buffer.BlockCopy(temtype, 0, temret, 0, temtype.Length);
                System.Buffer.BlockCopy(data, 0, temret, temtype.Length, data.Length);
                sendBuffer = temret;
            }catch{
                Console.WriteLine("ERROR: error when adding package type payload\n");
                sendBuffer = data;
                return false;
            }   
            // TODO: Define packet format
            return true;
        }

        /**
         * Check the payload of packet. Return false if any error.
         */
        public bool ParsePacket(byte[] recvBuffer, out byte[] destData, out PacketType packetType)
        {
            // TODO: Define packet format
            destData = new byte[recvBuffer.Length - payloadSize];
            try {
                System.Buffer.BlockCopy(recvBuffer, payloadSize, destData, 0, recvBuffer.Length - payloadSize);
                byte[] temtype = new byte[payloadSize];
                System.Buffer.BlockCopy(recvBuffer, 0, temtype, 0, payloadSize);
                PacketType[] pktArray = {
                    PacketType.SEARCH_QUERY,
                    PacketType.SEARCH_RESULT,
                    PacketType.MEDIA_INFO,
                    PacketType.VIDEO_STREAM,
                    PacketType.AUDIO_STREAM,
                    PacketType.SUBTITLE
                };       
                packetType = PacketType.UNDEFINED;     
                for (int i = 0; i < pktArray.Length; i++)
                {
                    byte[] typecomp = BitConverter.GetBytes((Int32)(pktArray[i])).Take(payloadSize).ToArray();
                    bool flag = true;
                    for (int k = 0; k < payloadSize; k++)
                    {
                       
                        if (temtype[k] != typecomp[k])
                        {
                            flag = false;
                            break;
                        }
                    }             
                    if (flag)
                    {
                        packetType = pktArray[i];
                        return true;
                    }
                }
            }
            catch
            {
                Console.WriteLine("ERROR: error when parsing package type payload\n");
            }        
            packetType = PacketType.UNDEFINED;
            return false;
        }
    }
}
