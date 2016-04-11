using FFmpeg.AutoGen;
using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public unsafe class VideoPlayer : Job
    {
        private VideoDecodeInfo videoDecodeInfo;
        private PlayerViewModel playerViewModel;
        private ManualResetEventSlim isVideoPlayingEvent;
        private double lastPts = 0;

        public VideoPlayer(VideoDecodeInfo videoDecodeInfo, PlayerViewModel playerViewModel, ManualResetEventSlim isVideoPlayingEvent)
        {
            this.videoDecodeInfo = videoDecodeInfo;
            this.playerViewModel = playerViewModel;
            this.isVideoPlayingEvent = isVideoPlayingEvent;
        }

        public void RunRepeatly(ManualResetEventSlim stopSignal, ManualResetEventSlim continueSignal)
        {
            Tuple<IntPtr, double> imageFramePtr = this.playerViewModel.PendingVideoFrames.Take();
            var pImageFrame = (AVFrame*)imageFramePtr.Item1.ToPointer();

            WriteImageToBuffer(pImageFrame);
            this.playerViewModel.AvailableImageBufferPool.Add(imageFramePtr.Item1);

            double pts = imageFramePtr.Item2;
            if (pts < lastPts)
            {
                lastPts = 0;
            }
            Thread.Sleep(TimeSpan.FromSeconds(pts - lastPts));
            this.lastPts = pts;
        }

        public void CleanUp()
        {

        }

        private void WriteImageToBuffer(AVFrame* pImageFrame)
        {
            var pImageBuffer = pImageFrame->data0;
            var imageBufferPtr = new IntPtr(pImageBuffer);
            var linesize = pImageFrame->linesize[0];

            this.playerViewModel.VideoScreenBitmap.Dispatcher.Invoke(() =>
            {
                this.playerViewModel.VideoScreenBitmap.Lock();
                Util.CopyMemory(this.playerViewModel.VideoScreenBitmap.BackBuffer, imageBufferPtr, videoDecodeInfo.ImageFrameBufferSize);
                this.playerViewModel.VideoScreenBitmap.AddDirtyRect(new Int32Rect(0, 0, videoDecodeInfo.Width, videoDecodeInfo.Height));
                this.playerViewModel.VideoScreenBitmap.Unlock();
            });
        }

        private void SaveBufferToFile()
        {
            this.playerViewModel.VideoScreenBitmap.Dispatcher.Invoke(() =>
            {
                using (FileStream stream = new FileStream("images/Screenshot.jpg", FileMode.Create))
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();

                    encoder.Frames.Add(BitmapFrame.Create(this.playerViewModel.VideoScreenBitmap));
                    encoder.Save(stream);
                }
            });
        }
    }
}
