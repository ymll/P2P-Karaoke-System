using AviFile;
using P2PKaraokeSystem.PlaybackLogic;
using P2PKaraokeSystem.View;
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

    public class PlayRequestListener : DataReceiveListener
    {
        public void OnDataReceived(PacketType packetType, byte[] data, String ipAddress, Int32 portNo)
        {
            Console.WriteLine(packetType);
            Console.WriteLine(data.Length);
            Console.WriteLine("PlayRequestListener OnDataReceived\n");
            String FilePath = "";
            byte[] temtype = new byte[4];
            System.Buffer.BlockCopy(data, 0, temtype, 0, temtype.Length);
            int reqType = BitConverter.ToInt32(temtype, 0);
            Console.WriteLine("request type = {0}", reqType); 


            for (int i = 4; i < data.Length; i++)
            {                
                FilePath += Convert.ToChar(data[i]);
            }
            Console.Write("FilePath = ");
            Console.WriteLine(FilePath);
            if (!File.Exists(FilePath))
            {
                return;
            }

           /*  * send the avi file to whom request*/
            string fileName = FilePath;

            Byte[] senddata;
            Console.WriteLine("Sending {0} to the host.", fileName);
            ClientSendManager c3 = new ClientSendManager(ipAddress, 12346);
            FileInfo file = new FileInfo(fileName);
            Console.WriteLine("Length {0}", file.Length);

            FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            int read;
            long totalWritten = 0;

            //type = 0, send 1,3,5,...blocks
            //type = 1, send 2,4,6,...blocks    
            //type = 2, send whole file
            int bufferSize = 1024 * 10 - 2;
            byte[] buffer = new byte[bufferSize];
            int counter = 0;


            while (true)
            {
                if (reqType < 2) fileStream.Seek((counter * 2 + reqType) * bufferSize, SeekOrigin.Begin);
                if(  ((read = fileStream.Read(buffer, 8, buffer.Length - 8))) > 0) break;
                counter++;
                byte[] temsize = BitConverter.GetBytes(totalWritten);
                System.Buffer.BlockCopy(temsize, 0, buffer, 0, temsize.Length);
                Console.WriteLine(file.Length - totalWritten);
                c3.AddPayload(out senddata, buffer, PacketType.VIDEO_STREAM);
                Console.WriteLine("send form {0} to {1}", totalWritten, totalWritten + buffer.Length - 8);
                c3.SendTCP(senddata, 0, read+10);
                totalWritten += read;
            //    Thread.Sleep(150);
            }
            Console.WriteLine("Return form sending ...");
        }
    }
}