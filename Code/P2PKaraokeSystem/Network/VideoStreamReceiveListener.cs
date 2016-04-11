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
        private static long upto = 0;
        public void OnDataReceived(PacketType packetType, byte[] data)
        {
            Console.WriteLine(packetType);
            Console.WriteLine("VideoStreamReceiveListener OnDataReceived\n");
         /*   for (int i = 0; i < (data.Length); i++)
            {
                Console.Write(data[i]);
            }*/
   
            FileStream fileStream = File.Open("../../VideoDatabase/Video/save.avi", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            fileStream.Seek(upto, SeekOrigin.Begin);
            fileStream.Write(data, 0, data.Length);
            upto += data.Length;
            Console.Write(upto);
            Console.WriteLine("");

            fileStream.Flush();
            fileStream.Dispose();
            fileStream.Close();
        }
    }
}