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
        public VideoDecodeInfo Video;
        public AudioDecodeInfo Audio;
        public AVFormatContext* pFormatContext;

        public MediaDecodeInfo()
        {
            this.Video = new VideoDecodeInfo();
            this.Audio = new AudioDecodeInfo();
            this.pFormatContext = ffmpeg.avformat_alloc_context();
        }
    }
}
