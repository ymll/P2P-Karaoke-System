using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public unsafe class VideoDecodeInfo : StreamDecodeInfo
    {
        public SwsContext* pConvertContext;
        public double Clock;
        public int Width { get { return pCodecContext->width; } }
        public int Height { get { return pCodecContext->height; } }
        public AVRational FrameRate { get { return pCodecContext->framerate; } }
        public int ImageFrameBufferSize { get; set; }

        public double GetClock()
        {
            double delta = (ffmpeg.av_gettime() - Clock) / 1000000.0;
            return Clock + delta;
        }
    }
}
