using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public interface DataSendListener
    {
        void OnDataAvailable(PacketType packetType, byte[] data);
    }
}
