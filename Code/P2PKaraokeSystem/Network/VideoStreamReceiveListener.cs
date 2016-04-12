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
        private long upto = 0;
        private string filename = "";
        private List<byte[]> UnUsedData;
        public void OnDataReceived(PacketType packetType, byte[] data, String ipAddress, Int32 portNo)
        {

            Console.WriteLine(packetType);
            Console.WriteLine("VideoStreamReceiveListener OnDataReceived\n");


            FileStream fileStream = File.Open("../../VideoDatabase/Video/save.avi", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            byte[] temsize = new byte[8];
            System.Buffer.BlockCopy(data, 0, temsize, 0, temsize.Length);
            long startpt = BitConverter.ToInt64(temsize, 0);
            //  Console.WriteLine("form {0} to {1}", startpt, startpt + data.Length - 8); 
            /*  for (int i = 0; i < (data.Length); i++)
              {
                         Console.Write(data[i]);
                     }*/
            fileStream.Seek(startpt, SeekOrigin.Begin);
            fileStream.Write(data, 8, data.Length - 8);
            upto += data.Length;
            //   Console.Write(upto);
            //     Console.WriteLine("");

            //     fileStream.Flush();
            //      fileStream.Dispose();

            fileStream.Close();

        }
    }
}