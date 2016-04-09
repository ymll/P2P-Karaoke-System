using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public class AudioLoader : StreamLoader
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
        }
    }
}
