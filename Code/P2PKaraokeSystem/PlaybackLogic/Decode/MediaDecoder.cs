using FFmpeg.AutoGen;
using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public unsafe class MediaDecoder
    {
        private VideoDecoder videoDecoder;

        private MediaDecodeInfo mediaDecodeInfo;
        private ManualResetEventSlim isVideoLoadedEvent;

        public MediaDecoder(MediaDecodeInfo mediaDecodeInfo, PlayerViewModel playerViewModel, ManualResetEventSlim isVideoLoadedEvent)
        {
            this.videoDecoder = new VideoDecoder(mediaDecodeInfo.Video, playerViewModel);

            this.mediaDecodeInfo = mediaDecodeInfo;
            this.isVideoLoadedEvent = isVideoLoadedEvent;
        }

        public void StartAsync()
        {
            new Thread(() =>
            {
                Start();
            }).Start();
        }

        private void Start()
        {
            AVPacket packet = new AVPacket();
            AVPacket* pPacket = &packet;

            ffmpeg.av_init_packet(pPacket);

            while (true)
            {
                this.isVideoLoadedEvent.Wait();

                if (ReadFrame(pPacket))
                {
                    if (packet.stream_index == mediaDecodeInfo.Video.pStream->index)
                    {
                        this.videoDecoder.Decode(pPacket);
                    }
                    else if (packet.stream_index == mediaDecodeInfo.Audio.pStream->index)
                    {

                    }
                }
            }
        }

        private bool ReadFrame(AVPacket* pPacket)
        {
            return ffmpeg.av_read_frame(mediaDecodeInfo.pFormatContext, pPacket) >= 0;
        }
    }
}
