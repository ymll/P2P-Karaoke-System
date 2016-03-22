using AviFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic
{
    class AviFileDecoder : IDecoder
    {
        private int _aviFile;

        private double _videoFrameRate;
        private uint _videoWidth;
        private uint _videoHeight;
        private int _firstVideoFrame;
        private int _countVideoFrames;

        private int _audioSampleSize;
        private int _audioNumOfBitsPerSample;
        private int _audioNumOfSamplesPerSecond;
        private int _audioNumOfChannels;
        private int _audioStreamLength;
        private int _firstAudioFrame;
        private int _countAudioFrames;
        private Avi.PCMWAVEFORMAT _audioFormatInfo;
        private IntPtr _aviAudioStream;
        private IntPtr _ptrAudioRawData;

        private PlayAudio playAudio = new PlayAudio();

        public void LoadFile(string filePath)
        {
            Avi.AVIFileInit();

            ThrowExceptionWhenResultNotZero("Cannot open the file as AVI video",
                Avi.AVIFileOpen(ref _aviFile, filePath, Avi.OF_READWRITE, 0));

            IntPtr aviVideoStream;

            var videoStreamInfo = GetVideoStreamInfo(out aviVideoStream);
            var audioStreamInfo = GetAudioStreamInfo(out this._aviAudioStream);
            var videoFormatInfo = GetVideoFormatInfo(aviVideoStream);
            this._audioFormatInfo = GetAudioFormatInfo(this._aviAudioStream);

            ParseVideoInfo(aviVideoStream, videoStreamInfo);
            ParseAudioInfo(audioStreamInfo, this._audioFormatInfo);
            playAudio.Play(this._audioFormatInfo);
        }

        public void UnLoadFile()
        {
            if (_aviFile != 0)
            {
                Avi.AVIFileRelease(_aviFile);
                Marshal.FreeHGlobal(_ptrAudioRawData);
            }
        }

        private Avi.AVISTREAMINFO GetVideoStreamInfo(out IntPtr aviVideoStream)
        {
            Avi.AVISTREAMINFO videoStreamInfo = new Avi.AVISTREAMINFO();

            ThrowExceptionWhenResultNotZero("Cannot get video stream",
                Avi.AVIFileGetStream(_aviFile, out aviVideoStream, Avi.streamtypeVIDEO, 0));

            ThrowExceptionWhenResultNotZero("Cannot get video stream info",
                Avi.AVIStreamInfo(aviVideoStream, ref videoStreamInfo, Marshal.SizeOf(videoStreamInfo)));

            return videoStreamInfo;
        }

        private Avi.AVISTREAMINFO GetAudioStreamInfo(out IntPtr aviAudioStream)
        {
            Avi.AVISTREAMINFO audioStreamInfo = new Avi.AVISTREAMINFO();

            ThrowExceptionWhenResultNotZero("Cannot get audio stream",
                Avi.AVIFileGetStream(_aviFile, out aviAudioStream, Avi.streamtypeAUDIO, 0));

            ThrowExceptionWhenResultNotZero("Cannot get video stream info",
                Avi.AVIStreamInfo(aviAudioStream, ref audioStreamInfo, Marshal.SizeOf(audioStreamInfo)));

            return audioStreamInfo;
        }

        private Avi.BITMAPINFO GetVideoFormatInfo(IntPtr aviVideoStream)
        {
            var bitmapInfo = new Avi.BITMAPINFO();
            int size = Marshal.SizeOf(bitmapInfo.bmiHeader);

            ThrowExceptionWhenResultNotZero("Cannot get video stream format",
                Avi.AVIStreamReadFormat(aviVideoStream, 0, ref bitmapInfo, ref size));

            return bitmapInfo;
        }

        private Avi.PCMWAVEFORMAT GetAudioFormatInfo(IntPtr aviAudioStream)
        {
            var pcmWaveFormat = new Avi.PCMWAVEFORMAT();
            int size = Marshal.SizeOf(pcmWaveFormat);

            ThrowExceptionWhenResultNotZero("Cannot get audio stream format",
                Avi.AVIStreamReadFormat(aviAudioStream, 0, ref pcmWaveFormat, ref size));

            return pcmWaveFormat;
        }

        private void ParseVideoInfo(IntPtr aviVideoStream, Avi.AVISTREAMINFO videoStreamInfo)
        {
            this._videoFrameRate = ((double)videoStreamInfo.dwRate) / videoStreamInfo.dwScale;
            this._videoWidth = videoStreamInfo.rcFrame.right;
            this._videoHeight = videoStreamInfo.rcFrame.bottom;
            this._firstVideoFrame = Avi.AVIStreamStart(aviVideoStream.ToInt32());
            this._countVideoFrames = Avi.AVIStreamLength(aviVideoStream.ToInt32());
        }

        private void ParseAudioInfo(Avi.AVISTREAMINFO audioStreamInfo, Avi.PCMWAVEFORMAT audioFormatInfo)
        {
            this._audioSampleSize = audioStreamInfo.dwSampleSize;
            this._audioNumOfBitsPerSample = audioFormatInfo.wBitsPerSample;
            this._audioNumOfSamplesPerSecond = audioFormatInfo.nSamplesPerSec;
            this._audioNumOfChannels = audioFormatInfo.nChannels;
            this._firstAudioFrame = Avi.AVIStreamStart(this._aviAudioStream.ToInt32());
            this._countAudioFrames = Avi.AVIStreamLength(this._aviAudioStream.ToInt32());

            this._audioStreamLength = this._audioNumOfBitsPerSample * this._audioNumOfSamplesPerSecond * this._audioNumOfChannels / 8 / 4;
            this._ptrAudioRawData = Marshal.AllocHGlobal(this._audioStreamLength);

            int startByte = this._firstAudioFrame;
            int endByte = this._firstAudioFrame + this._audioStreamLength;
            ThrowExceptionWhenResultNotZero("Cannot read audio stream",
                Avi.AVIStreamRead(this._aviAudioStream, startByte, endByte, this._ptrAudioRawData, this._audioStreamLength, 0, 0));
        }

        public bool ReadAudioFrame(TimeSpan timeSpan)
        {
            int startByteIndex = (int)(this._audioStreamLength * timeSpan.TotalSeconds);
            int endByteIndex = startByteIndex + this._audioStreamLength;

            System.Diagnostics.Trace.WriteLine(string.Format("Total Seconds: {0}", timeSpan.TotalSeconds));

            try
            {
                ReadAudioFrame(startByteIndex, endByteIndex);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ReadAudioFrame(int startByteIndex, int endByteIndex)
        {
            startByteIndex += this._firstAudioFrame;
            endByteIndex += this._firstAudioFrame;
            System.Diagnostics.Trace.WriteLine(string.Format("{0} ~ {1} [{2}~{3}]", startByteIndex, endByteIndex, this._firstAudioFrame, this._firstAudioFrame + this._countAudioFrames));

            ThrowExceptionWhenResultNotZero("Cannot read audio stream",
                Avi.AVIStreamRead(this._aviAudioStream, startByteIndex, endByteIndex, this._ptrAudioRawData, this._audioStreamLength, 0, 0));

            playAudio.WriteToStream(this._ptrAudioRawData, (endByteIndex - startByteIndex) * 4);
        }

        private void ThrowExceptionWhenResultNotZero(string errorMessage, int result)
        {
            if (result != 0)
            {
                throw new Exception(errorMessage);
            }
        }
    }
}
