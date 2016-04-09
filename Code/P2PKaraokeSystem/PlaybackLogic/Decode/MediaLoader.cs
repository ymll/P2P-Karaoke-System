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
        private MediaDecodeInfo decodeInfo;
        public StreamLoader Video { get; private set; }
        public StreamLoader Audio { get; private set; }

        public MediaLoader(MediaDecodeInfo decodeInfo, PlayerViewModel playerViewModel)
        {
            this.decodeInfo = decodeInfo;
            this.Video = new VideoLoader(decodeInfo.Video, playerViewModel);
            this.Audio = new AudioLoader(decodeInfo.Audio);
        }

        private void RetrieveFormatAndStreamInfo(string path)
        {
            fixed (AVFormatContext** ppFormatContext = &decodeInfo.pFormatContext)
            {
                Util.AssertZero("FFmpeg: Cannot open file",
                    ffmpeg.avformat_open_input(ppFormatContext, path, null, null));
            }

            Util.AssertZero("FFmpeg: Cannot find stream info",
                ffmpeg.avformat_find_stream_info(decodeInfo.pFormatContext, null));
        }

        private void RetrieveStreams()
        {
            for (var i = 0; i < decodeInfo.pFormatContext->nb_streams; i++)
            {
                var pStream = decodeInfo.pFormatContext->streams[i];

                // TODO: Handle multiple video/audio stream
                switch (pStream->codec->codec_type)
                {
                    case AVMediaType.AVMEDIA_TYPE_VIDEO:
                        decodeInfo.Video.pStream = pStream;
                        break;
                    case AVMediaType.AVMEDIA_TYPE_AUDIO:
                        decodeInfo.Audio.pStream = pStream;
                        break;
                }
            }

            if (decodeInfo.Video.pStream == null)
            {
                Trace.WriteLine("FFmpeg: File has no video stream");
            }

            if (decodeInfo.Audio.pStream == null)
            {
                Trace.WriteLine("FFmpeg: File has no audio stream");
            }

            Util.AssertTrue("FFmpeg: Cannot find any video or audio stream",
                decodeInfo.Video.pStream != null || decodeInfo.Audio.pStream != null);
        }

        private void LoadStreams()
        {
            Video.Load();
            Audio.Load();
        }

        public void Load(string path)
        {
            RetrieveFormatAndStreamInfo(path);
            RetrieveStreams();
            LoadStreams();
        }
    }
}
