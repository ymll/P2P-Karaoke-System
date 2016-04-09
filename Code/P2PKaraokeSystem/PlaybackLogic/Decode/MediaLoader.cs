using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public unsafe class MediaLoader
    {
        public MediaDecodeInfo DecodeInfo { get; private set; }

        public MediaLoader()
        {
            this.DecodeInfo = new MediaDecodeInfo();
        }

        public void RetrieveFormatAndStreamInfo(string path)
        {
            fixed (AVFormatContext** ppFormatContext = &DecodeInfo.pFormatContext)
            {
                Util.AssertZero("FFmpeg: Cannot open file",
                    ffmpeg.avformat_open_input(ppFormatContext, path, null, null));
            }

            Util.AssertZero("FFmpeg: Cannot find stream info",
                ffmpeg.avformat_find_stream_info(DecodeInfo.pFormatContext, null));
        }
    }
}
