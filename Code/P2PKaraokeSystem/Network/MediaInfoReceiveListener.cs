using AviFile;
using P2PKaraokeSystem.PlaybackLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace P2PKaraokeSystem.Network
{
    public class MediaInfoReceiveListener : DataReceiveListener
    {      
        public void OnDataReceived(PacketType packetType, byte[] data, String ipAddress, Int32 portNo)
        {
            Console.WriteLine("MediaInfoReceiveListener OnDataReceived\n");
           /* for (int i = 0; i < data.Length; i++){
                Console.Write(data[i]);
            }*/
            int dum = 4;
            byte[] destData = new byte[data.Length - dum];
            System.Buffer.BlockCopy(data, dum, destData, 0, data.Length - dum);
            byte[] teminfosize = new byte[dum];
            System.Buffer.BlockCopy(data, 0, teminfosize, 0, dum);
            data = destData;

            //Convert byte array to other types
            int aviInfoSize = BitConverter.ToInt32(teminfosize, 0);
            byte[] aviInfoData = new byte[aviInfoSize];
            byte[] formatInfoData = new byte[data.Length - aviInfoSize];
            System.Buffer.BlockCopy(data, 0, aviInfoData, 0, aviInfoSize);
            System.Buffer.BlockCopy(data, 0, formatInfoData, 0, data.Length - aviInfoSize);       
            Avi.AVISTREAMINFO avi_info = new Avi.AVISTREAMINFO();
            IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(aviInfoData, 0);
            avi_info = (Avi.AVISTREAMINFO)Marshal.PtrToStructure(ptr, avi_info.GetType());
            Avi.PCMWAVEFORMAT format_info = new Avi.PCMWAVEFORMAT();
            ptr = Marshal.UnsafeAddrOfPinnedArrayElement(formatInfoData, 0);
            format_info = (Avi.PCMWAVEFORMAT)Marshal.PtrToStructure(ptr, format_info.GetType());

           /* var header = new AudioHeaderReader();
            header.ParseMediaInfo(avi_info, format_info);*/

            Console.WriteLine("");
            Console.WriteLine(avi_info.dwLength);
            Console.WriteLine(format_info.nSamplesPerSec);
            Console.WriteLine("return from media info");
        }
    }
}
