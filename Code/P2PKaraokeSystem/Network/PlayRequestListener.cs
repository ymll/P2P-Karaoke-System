using AviFile;
using P2PKaraokeSystem.PlaybackLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
            Avi.AVISTREAMINFO avi_info = aviHeaderParser.AudioHeaderReader.StreamInfo;
            Avi.PCMWAVEFORMAT format_info = aviHeaderParser.AudioHeaderReader.FormatInfo;
            /*turn Avi.AVISTREAMINFO into byte array  */
            byte[] byteInfo = new byte[Marshal.SizeOf(avi_info) + Marshal.SizeOf(format_info)];
            IntPtr ptr;
            ptr = Marshal.AllocHGlobal(byteInfo.Length);
            Marshal.StructureToPtr(avi_info, ptr, false);
            int address = ptr.ToInt32();
            Marshal.Copy(new IntPtr(address), byteInfo, 0, Marshal.SizeOf(avi_info));
            Marshal.StructureToPtr(format_info, ptr, false);
            address = ptr.ToInt32();
            Marshal.Copy(new IntPtr(address), byteInfo, Marshal.SizeOf(avi_info), Marshal.SizeOf(format_info));  
            Marshal.FreeHGlobal(ptr);

            byte[] temsize = BitConverter.GetBytes((Int32)Marshal.SizeOf(avi_info)).Take(4).ToArray();
            byte[] temret = new byte[temsize.Length + byteInfo.Length];
            System.Buffer.BlockCopy(temsize, 0, temret, 0, temsize.Length);
            System.Buffer.BlockCopy(byteInfo, 0, temret, temsize.Length, byteInfo.Length);
            byteInfo = temret;

            byte[] sendBuffer = new byte[byteInfo.Length + 2];
            ServerSendManager sender = new ServerSendManager("127.0.0.1",12345);
            sender.AddPayload(out sendBuffer, byteInfo, PacketType.MEDIA_INFO);
            sender.SendTCP(sendBuffer, 0, sendBuffer.Length);
           
            /*  debugging: what is sent*/
            /* for (int i = 0; i < Marshal.SizeOf(avi_info); i++ )
            {
                Console.Write(byteInfo[i]);
            }*/
            /*Console.WriteLine("debug");*/
            /*
             * send the avi file to whom request
            string fileName = FilePath;

            Byte[] senddata;
            Console.WriteLine("Sending {0} to the host.", fileName);
            ServerSendManager c3 = new ServerSendManager("192.189.0.2",12345);
            FileInfo file = new FileInfo(fileName);
            Console.WriteLine("Length {0}", file.Length);

            FileStream fileStream = file.OpenRead();

            int read;
            int totalWritten = 0;

            byte[] buffer = new byte[32 * 1024]; // 32k chunks
            while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                c3.AddPayload(out senddata, buffer, PacketType.MEDIA_INFO);
                c3.SendTCP(senddata, 1, read);
                totalWritten += read;
            }
             */



            Console.WriteLine("Return form sending ...");
        }
    }
}
