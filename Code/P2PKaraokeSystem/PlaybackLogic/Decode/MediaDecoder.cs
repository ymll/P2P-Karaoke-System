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
    public unsafe class MediaDecoder : Job
    {
        private VideoDecoder videoDecoder;

        private MediaDecodeInfo mediaDecodeInfo;
        private PlayerViewModel playerViewModel;
        private double pts;

        public MediaDecoder(MediaDecodeInfo mediaDecodeInfo, PlayerViewModel playerViewModel)
        {
            this.videoDecoder = new VideoDecoder(mediaDecodeInfo.Video, playerViewModel);

            this.mediaDecodeInfo = mediaDecodeInfo;
            this.playerViewModel = playerViewModel;
        }

        public void RunRepeatly(ManualResetEventSlim stopSignal, ManualResetEventSlim continueSignal)
        {
            AVPacket packet = new AVPacket();
            AVPacket* pPacket = &packet;
            ffmpeg.av_init_packet(pPacket);

            if (ReadFrame(pPacket))
            {
                if (packet.stream_index == mediaDecodeInfo.Video.pStream->index)
                {
                    pts = this.videoDecoder.Decode(pPacket, pts);
                }
                else if (packet.stream_index == mediaDecodeInfo.Audio.pStream->index)
                {
                    playerViewModel.PendingAudioPackets.Add(packet);
                }
            }
        }

        public void CleanUp()
        {

        }

        private bool ReadFrame(AVPacket* pPacket)
        {
            return ffmpeg.av_read_frame(mediaDecodeInfo.pFormatContext, pPacket) >= 0;
        }
    }
}
