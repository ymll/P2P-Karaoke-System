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
        public int Width { get { return pCodecContext->width; } }
        public int Height { get { return pCodecContext->height; } }
        public int ImageFrameBufferSize { get; set; }
    }
}
