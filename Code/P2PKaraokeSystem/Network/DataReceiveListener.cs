using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public interface DataReceiveListener
    {
        void OnDataReceived(PacketType packetType, byte[] data, String ipAddress, Int32 portNo);
    }
}
