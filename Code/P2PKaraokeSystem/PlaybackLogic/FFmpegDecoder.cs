using FFmpeg.AutoGen;
using P2PKaraokeSystem.Model;
using P2PKaraokeSystem.PlaybackLogic.Decode;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace P2PKaraokeSystem.PlaybackLogic
{
    public unsafe class FFmpegDecoder
    {
        private PlayerViewModel playerViewModel;
        private PlaybackModel playbackModel;

        private ManualResetEventSlim isVideoLoadedEvent;
        private ManualResetEventSlim isVideoPlayingEvent;

        // Filled by RetrieveVideoCodecContextAndConvertContext()
        private AVRational frameRate;

        private MediaDecodeInfo mediaDecodeInfo;
        private MediaLoader mediaLoader;
        private MediaDecoder mediaDecoder;

        public FFmpegDecoder(PlayerViewModel playerViewModel, PlaybackModel playbackModel)
        {
            this.playerViewModel = playerViewModel;
            this.playbackModel = playbackModel;
            this.isVideoLoadedEvent = new ManualResetEventSlim(false);
            this.isVideoPlayingEvent = new ManualResetEventSlim(false);

            mediaDecodeInfo = new MediaDecodeInfo();
            mediaLoader = new MediaLoader(mediaDecodeInfo, playerViewModel);
            mediaDecoder = new MediaDecoder(mediaDecodeInfo, playerViewModel, isVideoLoadedEvent);

            playbackModel.PropertyChanged += playbackModel_PropertyChanged;

            this.StartPlaybackThread();
            mediaDecoder.StartAsync();
        }

        void playbackModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("CurrentVideo".Equals(e.PropertyName))
            {
                this.isVideoLoadedEvent.Reset();
                UnLoad();

                if (this.playbackModel.CurrentVideo != null)
                {
                    Load(this.playbackModel.CurrentVideo.FilePath);
                    this.isVideoLoadedEvent.Set();
                }
            }
            else if ("State".Equals(e.PropertyName))
            {
                switch (this.playbackModel.State)
                {
                    case PlayState.Playing:
                        this.isVideoPlayingEvent.Set();
                        break;
                    default:
                        this.isVideoPlayingEvent.Reset();
                        break;
                }
            }
        }

        private void Load(string path)
        {
            mediaLoader.RetrieveFormatAndStreamInfo(path);
            mediaLoader.RetrieveStreams();
            mediaLoader.LoadStreams();

            // For video stream
            this.frameRate = mediaDecodeInfo.Video.pCodecContext->framerate;

            this.playbackModel.Loaded = true;
        }

        private void StartPlaybackThread()
        {
            // Playback thread
            new Thread(() =>
            {
                while (true)
                {
                    this.isVideoPlayingEvent.Wait();

                    IntPtr imageFramePtr = this.playerViewModel.PendingVideoFrames.Take();
                    var pImageFrame = (AVFrame*)imageFramePtr.ToPointer();

                    WriteImageToBuffer(pImageFrame);
                    this.playerViewModel.AvailableImageBufferPool.Add(imageFramePtr);

                    int sleepTime = (int)(frameRate.den * 1000.0 / Math.Max(frameRate.num, 10));
                    Thread.Sleep(sleepTime);
                }
            }).Start();
        }

        private void UnLoad()
        {
            IntPtr imageFramePtr;
            while (this.playerViewModel.PendingVideoFrames.TryTake(out imageFramePtr)
                || this.playerViewModel.AvailableImageBufferPool.TryTake(out imageFramePtr))
            {
                var pImageFrame = (AVFrame*)imageFramePtr.ToPointer();
                ffmpeg.av_free(pImageFrame->data0);
                ffmpeg.av_frame_free(&pImageFrame);
            }

            ffmpeg.avcodec_close(mediaDecodeInfo.Video.pCodecContext);

            fixed (AVFormatContext** ppFormatContext = &mediaDecodeInfo.pFormatContext)
            {
                ffmpeg.avformat_close_input(ppFormatContext);
            }

            this.playbackModel.Loaded = false;
        }

        private void WriteImageToBuffer(AVFrame* pImageFrame)
        {
            var pImageBuffer = pImageFrame->data0;
            var imageBufferPtr = new IntPtr(pImageBuffer);
            var linesize = pImageFrame->linesize[0];

            this.playerViewModel.VideoScreenBitmap.Dispatcher.Invoke(() =>
            {
                this.playerViewModel.VideoScreenBitmap.Lock();
                CopyMemory(this.playerViewModel.VideoScreenBitmap.BackBuffer, imageBufferPtr, mediaDecodeInfo.Video.ImageFrameBufferSize);
                this.playerViewModel.VideoScreenBitmap.AddDirtyRect(new Int32Rect(0, 0, mediaDecodeInfo.Video.Width, mediaDecodeInfo.Video.Height));
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
