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
        private PlayerViewModel playerViewModel;
        private ManualResetEventSlim isVideoLoadedEvent;

        public MediaDecoder(MediaDecodeInfo mediaDecodeInfo, PlayerViewModel playerViewModel, ManualResetEventSlim isVideoLoadedEvent)
        {
            this.videoDecoder = new VideoDecoder(mediaDecodeInfo.Video, playerViewModel);

            this.mediaDecodeInfo = mediaDecodeInfo;
            this.playerViewModel = playerViewModel;
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
            double pts = 0;

            ffmpeg.av_init_packet(pPacket);

            while (true)
            {
                this.isVideoLoadedEvent.Wait();

                if (ReadFrame(pPacket))
                {
                    if (packet.stream_index == mediaDecodeInfo.Video.pStream->index)
                    {
                        pts = this.videoDecoder.Decode(pPacket, pts);
                    }
                    else if (packet.stream_index == mediaDecodeInfo.Audio.pStream->index)
                    {
                        AVPacket audioPacketCopy = new AVPacket();

                        Util.AssertZero("Cannot setup new packet",
                            ffmpeg.av_packet_ref(&audioPacketCopy, pPacket));

                        playerViewModel.PendingAudioPackets.Add(audioPacketCopy);
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
