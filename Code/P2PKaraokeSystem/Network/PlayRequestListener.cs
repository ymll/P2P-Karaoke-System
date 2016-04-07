using P2PKaraokeSystem.PlaybackLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public class PlayRequestListener : DataReceiveListener
    {
        public void OnDataReceived(PacketType packetType, byte[] data)
        {
            Console.WriteLine(packetType);
            Console.WriteLine(data.Length);
            Console.WriteLine("PlayRequestListener OnDataReceived\n");
            String FilePath = "";
            for (int i = 0; i < data.Length; i++)
            {                
                FilePath += Convert.ToChar(data[i]);
            }
            Console.Write("FilePath = ");
            Console.WriteLine(FilePath);
            var aviHeaderParser = new P2PKaraokeSystem.PlaybackLogic.AviHeaderParser();
            aviHeaderParser.LoadFile(FilePath);

        }
    }
}
