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
    public unsafe class VideoPlayer
    {
        private VideoDecodeInfo videoDecodeInfo;
        private PlayerViewModel playerViewModel;
        private ManualResetEventSlim isVideoPlayingEvent;

        public VideoPlayer(VideoDecodeInfo videoDecodeInfo, PlayerViewModel playerViewModel, ManualResetEventSlim isVideoPlayingEvent)
        {
            this.videoDecodeInfo = videoDecodeInfo;
            this.playerViewModel = playerViewModel;
            this.isVideoPlayingEvent = isVideoPlayingEvent;
        }

        public void Play()
        {
            this.isVideoPlayingEvent.Wait();

            IntPtr imageFramePtr = this.playerViewModel.PendingVideoFrames.Take();
            var pImageFrame = (AVFrame*)imageFramePtr.ToPointer();

            WriteImageToBuffer(pImageFrame);
            this.playerViewModel.AvailableImageBufferPool.Add(imageFramePtr);

            int sleepTime = (int)(videoDecodeInfo.FrameRate.den * 1000.0 / Math.Max(videoDecodeInfo.FrameRate.num, 10));
            Thread.Sleep(sleepTime);
        }

        private void WriteImageToBuffer(AVFrame* pImageFrame)
        {
            var pImageBuffer = pImageFrame->data0;
            var imageBufferPtr = new IntPtr(pImageBuffer);
            var linesize = pImageFrame->linesize[0];

            this.playerViewModel.VideoScreenBitmap.Dispatcher.Invoke(() =>
            {
                this.playerViewModel.VideoScreenBitmap.Lock();
                CopyMemory(this.playerViewModel.VideoScreenBitmap.BackBuffer, imageBufferPtr, videoDecodeInfo.ImageFrameBufferSize);
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

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);
    }
}
