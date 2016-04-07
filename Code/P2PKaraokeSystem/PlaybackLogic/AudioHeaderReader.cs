using AviFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic
{
    public class AudioHeaderReader : MediaHeaderReader<Avi.PCMWAVEFORMAT>
    {
        private int _audioSampleSize;
        private int _audioNumOfBitsPerSample;
        private int _audioNumOfSamplesPerSecond;
        private int _audioNumOfChannels;
        public int AudioStreamLength { get; private set; }
        public int FirstAudioFrame { get; private set; }
        public int FrameCount { get; private set; }

        public override Avi.AVISTREAMINFO GetStreamInfo(int aviFile)
        {
            IntPtr stream;
            Avi.AVISTREAMINFO audioStreamInfo = new Avi.AVISTREAMINFO();

            Util.AssertZero("Cannot get audio stream",
                Avi.AVIFileGetStream(aviFile, out stream, Avi.streamtypeAUDIO, 0));

            this.Stream = stream;
            Util.AssertZero("Cannot get video stream info",
                Avi.AVIStreamInfo(this.Stream, ref audioStreamInfo, Marshal.SizeOf(audioStreamInfo)));

            return audioStreamInfo;
        }

        public override Avi.PCMWAVEFORMAT GetFormatInfo(IntPtr stream)
        {
            var pcmWaveFormat = new Avi.PCMWAVEFORMAT();
            int size = Marshal.SizeOf(pcmWaveFormat);

            Util.AssertZero("Cannot get audio stream format",
                Avi.AVIStreamReadFormat(stream, 0, ref pcmWaveFormat, ref size));

            return pcmWaveFormat;
        }

        public override void ParseMediaInfo(Avi.AVISTREAMINFO audioStreamInfo, Avi.PCMWAVEFORMAT audioFormatInfo)
        {
            this._audioSampleSize = audioStreamInfo.dwSampleSize;
            this._audioNumOfBitsPerSample = audioFormatInfo.wBitsPerSample;
            this._audioNumOfSamplesPerSecond = audioFormatInfo.nSamplesPerSec;
            this._audioNumOfChannels = audioFormatInfo.nChannels;
            this.FirstAudioFrame = Avi.AVIStreamStart(this.Stream.ToInt32());
            this.FrameCount = Avi.AVIStreamLength(this.Stream.ToInt32());

            this.AudioStreamLength = this.FrameCount * this._audioSampleSize;
        }
    }
}
