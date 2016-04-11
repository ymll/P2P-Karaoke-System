using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public class SubtitleReceiveListener : DataReceiveListener
    {
        public void OnDataReceived(PacketType packetType, byte[] data)
        {
            Console.WriteLine(packetType);
            Console.WriteLine("SubtitleReceiveListener OnDataReceived\n");

            FileStream fileStream = File.Open("../../VideoDatabase/Lyrics/save.lrc", FileMode.Append);
            fileStream.Write(data, 0, data.Length);
            fileStream.Dispose();
        }
    }
}