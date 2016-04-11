using FFmpeg.AutoGen;
using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public unsafe class VideoDecoder
    {
        private VideoDecodeInfo videoDecodeInfo;
        private PlayerViewModel playerViewModel;

        public VideoDecoder(VideoDecodeInfo videoDecodeInfo, PlayerViewModel playerViewModel)
        {
            this.videoDecodeInfo = videoDecodeInfo;
            this.playerViewModel = playerViewModel;
        }

        public double Decode(AVPacket* pPacket, double pts)
        {
            bool gotPicture = DecodeFrame(pPacket);
            double newPts = GetPts(pPacket);

            if (gotPicture)
            {
                IntPtr imageFramePtr = this.playerViewModel.AvailableImageBufferPool.Take();
                var pImageFrame = (AVFrame*)imageFramePtr.ToPointer();

                ConvertFrameToImage(pImageFrame);

                newPts = SynchronizeVideo(pImageFrame, newPts);
                this.playerViewModel.PendingVideoFrames.Add(new Tuple<IntPtr, double>(imageFramePtr, newPts));
            }

            return newPts;
        }

        private bool DecodeFrame(AVPacket* pVideoPacket)
        {
            var gotPicture = 0;
            Util.AssertNonNegative("FFmpeg: Cannot decode video frame",
                ffmpeg.avcodec_decode_video2(videoDecodeInfo.pCodecContext, videoDecodeInfo.pFrame, &gotPicture, pVideoPacket));

            return gotPicture == 1;
        }

        private double GetPts(AVPacket* pPacket)
        {
            double pts;

            if (pPacket->dts == ffmpeg.AV_NOPTS_VALUE
                && videoDecodeInfo.pFrame->opaque != null
                && *(long*)videoDecodeInfo.pFrame->opaque != ffmpeg.AV_NOPTS_VALUE)
            {
                pts = *(ulong*)videoDecodeInfo.pFrame->opaque;
            }
            else if (pPacket->dts != ffmpeg.AV_NOPTS_VALUE)
            {
                pts = pPacket->dts;
            }
            else
            {
                pts = 0;
            }
            pts *= q2d(videoDecodeInfo.pCodecContext->time_base);

            return pts;
        }

        private double SynchronizeVideo(AVFrame* pFrame, double pts)
        {
            if (pts != 0)
            {
                videoDecodeInfo.Clock = pts;
            }
            else
            {
                pts = videoDecodeInfo.Clock;
            }

            double frame_delay = q2d(videoDecodeInfo.pCodecContext->time_base);
            frame_delay += pFrame->repeat_pict * (frame_delay * 0.5);
            videoDecodeInfo.Clock += frame_delay;

            return pts;
        }

        private void ConvertFrameToImage(AVFrame* pImageFrame)
        {
            var src = &videoDecodeInfo.pFrame->data0;
            var dest = &pImageFrame->data0;
            var srcStride = videoDecodeInfo.pFrame->linesize;
            var destStride = pImageFrame->linesize;

            ffmpeg.sws_scale(videoDecodeInfo.pConvertContext, src, srcStride, 0, videoDecodeInfo.Height, dest, destStride);
        }

        private double q2d(AVRational a)
        {
            return a.num / (double)a.den;
        }
    }
}
