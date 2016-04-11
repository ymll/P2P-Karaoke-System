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

        private MediaDecodeInfo mediaDecodeInfo;
        private MediaLoader mediaLoader;
        private MediaDecoder mediaDecoder;

        private JobThread audioDecoderThread;
        private JobThread videoPlayerThread;

        public FFmpegDecoder(PlayerViewModel playerViewModel, PlaybackModel playbackModel)
        {
            this.playerViewModel = playerViewModel;
            this.playbackModel = playbackModel;
            this.isVideoLoadedEvent = new ManualResetEventSlim(false);
            this.isVideoPlayingEvent = new ManualResetEventSlim(false);

            mediaDecodeInfo = new MediaDecodeInfo();
            mediaLoader = new MediaLoader(mediaDecodeInfo, playerViewModel);
            mediaDecoder = new MediaDecoder(mediaDecodeInfo, playerViewModel, isVideoLoadedEvent);

            var audioDecoder = new AudioDecoder(mediaDecodeInfo.Audio, mediaDecodeInfo, playerViewModel);

            var videoPlayer = new VideoPlayer(mediaDecodeInfo.Video, playerViewModel, isVideoPlayingEvent);
            var audioPlayer = new AudioPlayer(mediaDecodeInfo.Audio, playerViewModel, playbackModel);

            audioDecoderThread = new JobThread("Audio Decoder", audioDecoder, null, null);
            videoPlayerThread = new JobThread("Video Player", videoPlayer, null, isVideoPlayingEvent);

            playbackModel.PropertyChanged += playbackModel_PropertyChanged;
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
            mediaLoader.Load(path);
            this.playbackModel.Loaded = true;
        }

        public void StartAsync()
        {
            videoPlayerThread.Start();
            audioDecoderThread.Start();
            mediaDecoder.StartAsync();
        }

        private void UnLoad()
        {
            Tuple<IntPtr, double> imageFramePtrWithTime;
            IntPtr imageFramePtr;
            while (this.playerViewModel.PendingVideoFrames.TryTake(out imageFramePtrWithTime))
            {
                var pImageFrame = (AVFrame*)imageFramePtrWithTime.Item1.ToPointer();
                ffmpeg.av_free(pImageFrame->data0);
                ffmpeg.av_frame_free(&pImageFrame);
            }

            while (this.playerViewModel.AvailableImageBufferPool.TryTake(out imageFramePtr))
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
    }
}
