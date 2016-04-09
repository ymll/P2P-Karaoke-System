using FFmpeg.AutoGen;
using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public unsafe class MediaLoader
    {
        public MediaDecodeInfo DecodeInfo { get; private set; }
        public StreamLoader Video { get; private set; }
        public StreamLoader Audio { get; private set; }

        public MediaLoader(PlayerViewModel playerViewModel)
        {
            this.DecodeInfo = new MediaDecodeInfo();
            this.Video = new VideoLoader(DecodeInfo.Video, playerViewModel);
            this.Audio = new AudioLoader(DecodeInfo.Audio);
        }

        public void RetrieveFormatAndStreamInfo(string path)
        {
            fixed (AVFormatContext** ppFormatContext = &DecodeInfo.pFormatContext)
            {
                Util.AssertZero("FFmpeg: Cannot open file",
                    ffmpeg.avformat_open_input(ppFormatContext, path, null, null));
            }

            Util.AssertZero("FFmpeg: Cannot find stream info",
                ffmpeg.avformat_find_stream_info(DecodeInfo.pFormatContext, null));
        }

        public void RetrieveStreams()
        {
            for (var i = 0; i < DecodeInfo.pFormatContext->nb_streams; i++)
            {
                var pStream = DecodeInfo.pFormatContext->streams[i];

                // TODO: Handle multiple video/audio stream
                switch (pStream->codec->codec_type)
                {
                    case AVMediaType.AVMEDIA_TYPE_VIDEO:
                        DecodeInfo.Video.pStream = pStream;
                        break;
                    case AVMediaType.AVMEDIA_TYPE_AUDIO:
                        DecodeInfo.Audio.pStream = pStream;
                        break;
                }
            }

            if (DecodeInfo.Video.pStream == null)
            {
                Trace.WriteLine("FFmpeg: File has no video stream");
            }

            if (DecodeInfo.Audio.pStream == null)
            {
                Trace.WriteLine("FFmpeg: File has no audio stream");
            }

            Util.AssertTrue("FFmpeg: Cannot find any video or audio stream",
                DecodeInfo.Video.pStream != null || DecodeInfo.Audio.pStream != null);
        }

        public void LoadStreams()
        {
            Video.Load();
            Audio.Load();
        }
    }
}
