using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace P2PKaraokeSystem.Network
{
    public class VideoStreamReceiveListener : DataReceiveListener
    {
        public void OnDataReceived(PacketType packetType, byte[] data)
        {
            Console.WriteLine(packetType);
            Console.WriteLine("VideoStreamReceiveListener OnDataReceived\n");
         /*   for (int i = 0; i < (data.Length); i++)
            {
                Console.Write(data[i]);
            }*/
            FileStream fileStream = File.Open("../../VideoDatabase/Video/save.avi", FileMode.Append,FileAccess.Write,FileShare.ReadWrite);    
            fileStream.Write(data, 0, data.Length);    
            fileStream.Close();
          //        fileStream.Flush();
         //  fileStream.Dispose();

        }
    }
}
