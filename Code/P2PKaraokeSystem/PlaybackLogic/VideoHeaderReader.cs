using AviFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic
{
    public class VideoHeaderReader : MediaHeaderReader<Avi.BITMAPINFO>
    {
        public double VideoFrameRate { get; private set; }
        public uint VideoWidth { get; private set; }
        public uint VideoHeight { get; private set; }
        public int FirstFrame { get; private set; }
        public int FrameCount { get; private set; }

        public override Avi.AVISTREAMINFO GetStreamInfo(int aviFile)
        {
            IntPtr stream;
            Avi.AVISTREAMINFO videoStreamInfo = new Avi.AVISTREAMINFO();

            Util.AssertZero("Cannot get video stream",
                Avi.AVIFileGetStream(aviFile, out stream, Avi.streamtypeVIDEO, 0));

            this.Stream = stream;
            Util.AssertZero("Cannot get video stream info",
                Avi.AVIStreamInfo(this.Stream, ref videoStreamInfo, Marshal.SizeOf(videoStreamInfo)));

            return videoStreamInfo;
        }

        public override Avi.BITMAPINFO GetFormatInfo(IntPtr aviStream)
        {
            var bitmapInfo = new Avi.BITMAPINFO();
            int size = Marshal.SizeOf(bitmapInfo.bmiHeader);

            Util.AssertZero("Cannot get video stream format",
                Avi.AVIStreamReadFormat(aviStream, 0, ref bitmapInfo, ref size));

            return bitmapInfo;
        }

        public override void ParseMediaInfo(Avi.AVISTREAMINFO streamInfo, Avi.BITMAPINFO formatInfo)
        {
            this.VideoFrameRate = ((double)streamInfo.dwRate) / streamInfo.dwScale;
            this.VideoWidth = streamInfo.rcFrame.right;
            this.VideoHeight = streamInfo.rcFrame.bottom;
            this.FirstFrame = Avi.AVIStreamStart(this.Stream.ToInt32());
            this.FrameCount = Avi.AVIStreamLength(this.Stream.ToInt32());
        }
    }
}
