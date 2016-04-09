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
        private bool IS_FRAME_SAVE_TO_FILE = false;

        private PlayerViewModel playerViewModel;
        private PlaybackModel playbackModel;
        private int currentFrame;

        private ManualResetEventSlim isVideoLoadedEvent;
        private ManualResetEventSlim isVideoPlayingEvent;

        // Filled by RetrieveVideoCodecContextAndConvertContext()
        private AVRational frameRate;

        private MediaLoader mediaLoader;

        public FFmpegDecoder(PlayerViewModel playerViewModel, PlaybackModel playbackModel)
        {
            this.playerViewModel = playerViewModel;
            this.playbackModel = playbackModel;
            this.isVideoLoadedEvent = new ManualResetEventSlim(false);
            this.isVideoPlayingEvent = new ManualResetEventSlim(false);

            mediaLoader = new MediaLoader(playerViewModel);

            playbackModel.PropertyChanged += playbackModel_PropertyChanged;

            this.StartPlaybackThread();
            this.StartDecodeThread();
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
            currentFrame = 0;

            mediaLoader.RetrieveFormatAndStreamInfo(path);
            mediaLoader.RetrieveStreams();
            mediaLoader.LoadStreams();

            // For video stream
            this.frameRate = mediaLoader.DecodeInfo.Video.pCodecContext->framerate;

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

        public void StartDecodeThread()
        {
            new Thread(() =>
            {
                AVPacket packet = new AVPacket();
                AVPacket* pPacket = &packet;

                ffmpeg.av_init_packet(pPacket);

                while (true)
                {
                    this.isVideoLoadedEvent.Wait();

                    if (ReadFrame(pPacket))
                    {
                        if (packet.stream_index == mediaLoader.DecodeInfo.Video.pStream->index)
                        {
                            if (DecodeVideoFrame(pPacket))
                            {
                                IntPtr imageFramePtr = this.playerViewModel.AvailableImageBufferPool.Take();
                                var pImageFrame = (AVFrame*)imageFramePtr.ToPointer();

                                ConvertFrameToImage(pImageFrame);

                                if (IS_FRAME_SAVE_TO_FILE && currentFrame % 20 == 0)
                                {
                                    SaveBufferToFile();
                                }

                                this.playerViewModel.PendingVideoFrames.Add(imageFramePtr);
                            }
                        }
                        else if (packet.stream_index == mediaLoader.DecodeInfo.Audio.pStream->index)
                        {

                        }
                    }
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

            ffmpeg.avcodec_close(mediaLoader.DecodeInfo.Video.pCodecContext);

            fixed (AVFormatContext** ppFormatContext = &mediaLoader.DecodeInfo.pFormatContext)
            {
                ffmpeg.avformat_close_input(ppFormatContext);
            }

            this.playbackModel.Loaded = false;
        }

        private bool ReadFrame(AVPacket* pPacket)
        {
            currentFrame++;
            return ffmpeg.av_read_frame(mediaLoader.DecodeInfo.pFormatContext, pPacket) >= 0;
        }

        private bool DecodeVideoFrame(AVPacket* pVideoPacket)
        {
            var gotPicture = 0;
            Util.AssertNonNegative("FFmpeg: Cannot decode video frame",
                ffmpeg.avcodec_decode_video2(mediaLoader.DecodeInfo.Video.pCodecContext, mediaLoader.DecodeInfo.Video.pFrame, &gotPicture, pVideoPacket));

            return gotPicture == 1;
        }

        private void ConvertFrameToImage(AVFrame* pImageFrame)
        {
            var src = &mediaLoader.DecodeInfo.Video.pFrame->data0;
            var dest = &pImageFrame->data0;
            var srcStride = mediaLoader.DecodeInfo.Video.pFrame->linesize;
            var destStride = pImageFrame->linesize;

            ffmpeg.sws_scale(mediaLoader.DecodeInfo.Video.pConvertContext, src, srcStride, 0, mediaLoader.DecodeInfo.Video.Height, dest, destStride);
        }

        private void WriteImageToBuffer(AVFrame* pImageFrame)
        {
            var pImageBuffer = pImageFrame->data0;
            var imageBufferPtr = new IntPtr(pImageBuffer);
            var linesize = pImageFrame->linesize[0];

            this.playerViewModel.VideoScreenBitmap.Dispatcher.Invoke(() =>
            {
                this.playerViewModel.VideoScreenBitmap.Lock();
                CopyMemory(this.playerViewModel.VideoScreenBitmap.BackBuffer, imageBufferPtr, mediaLoader.DecodeInfo.Video.ImageFrameBufferSize);
                this.playerViewModel.VideoScreenBitmap.AddDirtyRect(new Int32Rect(0, 0, mediaLoader.DecodeInfo.Video.Width, mediaLoader.DecodeInfo.Video.Height));
                this.playerViewModel.VideoScreenBitmap.Unlock();
            });
        }

        private void SaveBufferToFile()
        {
            this.playerViewModel.VideoScreenBitmap.Dispatcher.Invoke(() =>
            {
                using (FileStream stream = new FileStream("images/" + currentFrame + ".jpg", FileMode.Create))
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
