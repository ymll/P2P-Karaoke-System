using AviFile;
using P2PKaraokeSystem.PlaybackLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace P2PKaraokeSystem.Network
{  

    public class LyricRequestListener : DataReceiveListener
    {
        public void OnDataReceived(PacketType packetType, byte[] data, String ipAddress, Int32 portNo)
        {
            Console.WriteLine(packetType);
            Console.WriteLine(data.Length);
            Console.WriteLine("LyricRequestListener OnDataReceived\n");
            String FilePath = "";
            for (int i = 0; i < data.Length; i++)
            {                
                FilePath += Convert.ToChar(data[i]);
            }
            Console.Write("FilePath = ");
            Console.WriteLine(FilePath);   
            string fileName = FilePath;

            Byte[] senddata;
            Console.WriteLine("Sending {0} to the host.", fileName);
            ServerSendManager c3 = new ServerSendManager();
         //   c3.NewReceiver("192.168.0.5", 12345);
            c3.NewReceiver("127.0.0.1",12345);
            FileInfo file = new FileInfo(fileName);
            Console.WriteLine("Length {0}", file.Length);
            FileStream fileStream = file.OpenRead();
            int read;
            int totalWritten = 0;

            byte[] buffer = new byte[1024 * 10 - 2];
            while (((read = fileStream.Read(buffer, 8, buffer.Length - 8))) > 0)
            {
                byte[] temsize = BitConverter.GetBytes(totalWritten);
                System.Buffer.BlockCopy(temsize, 0, buffer, 0, temsize.Length);
                Console.WriteLine(file.Length - totalWritten);
                c3.AddPayload(out senddata, buffer, PacketType.SUBTITLE);
                c3.SendTCP(senddata, 0, read + 10);
                totalWritten += read;
            }
         
            Console.WriteLine("Return form sending ...");
        }
    }
}
