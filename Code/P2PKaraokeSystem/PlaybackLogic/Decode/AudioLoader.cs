using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public unsafe class AudioLoader : StreamLoader
    {
        private AudioDecodeInfo audioDecodeInfo;

        public AudioLoader(AudioDecodeInfo audioDecodeInfo)
            : base(audioDecodeInfo)
        {
            this.audioDecodeInfo = audioDecodeInfo;
        }

        public override void Load()
        {
            FindAndOpenDecoder();
            PrepareResampleContext();
        }

        private void PrepareResampleContext()
        {
            audioDecodeInfo.pResampleContext = ffmpeg.swr_alloc();

            Util.AssertTrue("FFmpeg: Cannot initialize conversion context",
                audioDecodeInfo.pResampleContext != null);
        }
    }
}
