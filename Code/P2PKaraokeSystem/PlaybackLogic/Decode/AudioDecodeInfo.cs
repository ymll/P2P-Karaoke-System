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

        public int Frequency { get { return pCodecContext->sample_rate; } }
        public int NumOfChannels { get { return pCodecContext->channels; } }

        public AudioDecodeInfo()
        {
            this.AudioOut = new IntPtr();
        }
    }
}
