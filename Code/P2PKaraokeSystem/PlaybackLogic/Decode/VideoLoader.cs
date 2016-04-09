using FFmpeg.AutoGen;
using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public unsafe class VideoLoader : StreamLoader
    {
        private VideoDecodeInfo videoDecodeInfo;
        private PlayerViewModel playerViewModel;

        private const AVPixelFormat DISPLAY_COLOR_FORMAT = AVPixelFormat.AV_PIX_FMT_BGRA;
        private PixelFormat WRITEABLE_BITMAP_FORMAT = PixelFormats.Bgr32;

        public VideoLoader(VideoDecodeInfo videoDecodeInfo, PlayerViewModel playerViewModel)
            : base(videoDecodeInfo)
        {
            this.videoDecodeInfo = videoDecodeInfo;
            this.playerViewModel = playerViewModel;
        }

        public override void Load()
        {
            FindAndOpenDecoder();

            videoDecodeInfo.pConvertContext = ffmpeg.sws_getContext(
                videoDecodeInfo.Width,
                videoDecodeInfo.Height,
                videoDecodeInfo.pCodecContext->pix_fmt,
                videoDecodeInfo.Width,
                videoDecodeInfo.Height,
                DISPLAY_COLOR_FORMAT,
                ffmpeg.SWS_FAST_BILINEAR, null, null, null
            );

            Util.AssertTrue("FFmpeg: Cannot initialize conversion context",
                videoDecodeInfo.pConvertContext != null);

            PrepareImageFrameAndBuffer();
        }

        private void PrepareImageFrameAndBuffer()
        {
            videoDecodeInfo.ImageFrameBufferSize = ffmpeg.avpicture_get_size(DISPLAY_COLOR_FORMAT, videoDecodeInfo.Width, videoDecodeInfo.Height);
            var numberOfBuffer = Math.Max(2, this.playerViewModel.MaxBufferSizeInMegabyte * 1024 * 1024 / videoDecodeInfo.ImageFrameBufferSize);

            for (int i = 0; i < numberOfBuffer; i++)
            {
                var pImageFrame = ffmpeg.av_frame_alloc();
                var pImageBuffer = (sbyte*)ffmpeg.av_malloc((ulong)videoDecodeInfo.ImageFrameBufferSize);
                var imageFramePtr = new IntPtr(pImageFrame);

                ffmpeg.avpicture_fill((AVPicture*)pImageFrame, pImageBuffer, DISPLAY_COLOR_FORMAT, videoDecodeInfo.Width, videoDecodeInfo.Height);
                this.playerViewModel.AvailableImageBufferPool.Add(imageFramePtr);
            }

            this.playerViewModel.VideoScreenBitmap = new WriteableBitmap(videoDecodeInfo.Width, videoDecodeInfo.Height, 72, 72, WRITEABLE_BITMAP_FORMAT, null);
        }
    }
}
