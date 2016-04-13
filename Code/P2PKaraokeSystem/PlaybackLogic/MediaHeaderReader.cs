using AviFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic
{
    public abstract class MediaHeaderReader<T>
    {
        public IntPtr Stream { get; protected set; }
        public Avi.AVISTREAMINFO StreamInfo { get; protected set; }
        public T FormatInfo { get; protected set; }

        public void Load(int aviFile)
        {
            this.StreamInfo = GetStreamInfo(aviFile);
            this.FormatInfo = GetFormatInfo(this.Stream);
            ParseMediaInfo(this.StreamInfo, this.FormatInfo);
        }

        public abstract Avi.AVISTREAMINFO GetStreamInfo(int aviFile);

        public abstract T GetFormatInfo(IntPtr stream);

        public abstract void ParseMediaInfo(Avi.AVISTREAMINFO streamInfo, T formatInfo);
    }
}
