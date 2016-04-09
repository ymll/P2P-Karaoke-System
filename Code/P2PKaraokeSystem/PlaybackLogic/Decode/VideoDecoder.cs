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

        public void Decode(AVPacket* pPacket)
        {
            if (DecodeFrame(pPacket))
            {
                IntPtr imageFramePtr = this.playerViewModel.AvailableImageBufferPool.Take();
                var pImageFrame = (AVFrame*)imageFramePtr.ToPointer();

                ConvertFrameToImage(pImageFrame);

                this.playerViewModel.PendingVideoFrames.Add(imageFramePtr);
            }
        }

        private bool DecodeFrame(AVPacket* pVideoPacket)
        {
            var gotPicture = 0;
            Util.AssertNonNegative("FFmpeg: Cannot decode video frame",
                ffmpeg.avcodec_decode_video2(videoDecodeInfo.pCodecContext, videoDecodeInfo.pFrame, &gotPicture, pVideoPacket));

            return gotPicture == 1;
        }

        private void ConvertFrameToImage(AVFrame* pImageFrame)
        {
            var src = &videoDecodeInfo.pFrame->data0;
            var dest = &pImageFrame->data0;
            var srcStride = videoDecodeInfo.pFrame->linesize;
            var destStride = pImageFrame->linesize;

            ffmpeg.sws_scale(videoDecodeInfo.pConvertContext, src, srcStride, 0, videoDecodeInfo.Height, dest, destStride);
        }
    }
}
