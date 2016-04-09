using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public unsafe class AudioDecodeInfo : StreamDecodeInfo
    {
        public IntPtr AudioOut;
        public SwrContext* pResampleContext;

        public double Clock;
        public double DiffCum;
        public double DiffAvgCoef;
        public int DiffAvgCount;
        public double DiffThreshold;
        public const int AUDIO_DIFF_AVG_NB = 20;
        public const int SAMPLE_CORRECTION_PERCENT_MAX = 10;

        public AVPacket* pPacket;
        public sbyte* PacketData;
        public int PacketSize;

        public byte[] Buffer;
        public int BufferCurrentIndex { get; set; }
        public int BufferSize { get; set; }
        public const int MAX_BUFFER_SIZE = 1024;
        public const int MAX_AUDIO_FRAME_SIZE = 192000;

        public int Frequency { get { return pCodecContext->sample_rate; } }
        public int NumOfChannels { get { return pCodecContext->channels; } }

        public AudioDecodeInfo()
        {
            this.AudioOut = new IntPtr();
            this.Buffer = new byte[(MAX_AUDIO_FRAME_SIZE * 3) / 2];
        }

        public double GetClock()
        {
            double pts;
            int hw_buf_size, bytes_per_sec, n;

            pts = Clock;
            hw_buf_size = BufferSize - BufferCurrentIndex;
            bytes_per_sec = 0;
            n = pCodecContext->channels * 2;
            bytes_per_sec = pCodecContext->sample_rate * n;

            if (bytes_per_sec != 0)
            {
                pts -= (double)hw_buf_size / bytes_per_sec;
            }
            return pts;
        }
    }
}
