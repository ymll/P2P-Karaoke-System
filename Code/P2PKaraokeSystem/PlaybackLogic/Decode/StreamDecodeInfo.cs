using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public unsafe class StreamDecodeInfo
    {
        public AVStream* pStream { get; set; }
        public AVCodecContext* pCodecContext { get; set; }
        public AVFrame* pFrame;

        public StreamDecodeInfo()
        {
            pFrame = ffmpeg.av_frame_alloc();
        }
    }
}
