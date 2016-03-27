using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Native.FFmpeg
{
    public class FFmpegLoader
    {
        public static void LoadFFmpeg()
        {
            var ffmpegPath = string.Format(@"lib/{0}", Environment.Is64BitProcess ? @"x64" : @"x86");
            SetDllDirectory(ffmpegPath);

            ffmpeg.av_register_all();
            ffmpeg.avcodec_register_all();
            ffmpeg.avformat_network_init();
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
    }
}
