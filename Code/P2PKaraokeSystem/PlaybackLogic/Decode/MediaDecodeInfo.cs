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

        public long LengthInMillisecond
        {
            get { return pFormatContext->duration * 1000 / ffmpeg.AV_TIME_BASE; }
        }

        public static AVPacket FlushPacket;
        public double Clock;

        public SyncType Sync = SyncType.VIDEO;
        public const int AV_NOSYNC_THRESHOLD = 10;

        static MediaDecodeInfo()
        {
            FlushPacket = new AVPacket();
            sbyte[] data = new sbyte[0];
            fixed (sbyte* pData = data)
            {
                FlushPacket.data = pData;
            }
        }

        public MediaDecodeInfo()
        {
            this.Video = new VideoDecodeInfo();
            this.Audio = new AudioDecodeInfo();
            this.pFormatContext = ffmpeg.avformat_alloc_context();
        }

        public double GetMasterClock()
        {
            switch (Sync)
            {
                case SyncType.VIDEO:
                    return Video.GetClock();
                case SyncType.AUDIO:
                    return Audio.GetClock();
                case SyncType.EXTERNAL:
                    return ffmpeg.av_gettime() / 1000000.0;
                default:
                    return -1;
            }
        }

        public enum SyncType { VIDEO, AUDIO, EXTERNAL }
    }
}
