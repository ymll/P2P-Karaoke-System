using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public unsafe class MediaDecodeInfo
    {
        public AVFormatContext* pFormatContext;

        public MediaDecodeInfo()
        {
            this.pFormatContext = ffmpeg.avformat_alloc_context();
        }
    }
}
