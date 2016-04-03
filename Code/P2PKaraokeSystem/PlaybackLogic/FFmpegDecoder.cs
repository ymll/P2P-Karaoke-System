using FFmpeg.AutoGen;
using P2PKaraokeSystem.Model;
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
        private const AVPixelFormat DISPLAY_COLOR_FORMAT = AVPixelFormat.AV_PIX_FMT_BGRA;
        private PixelFormat WRITEABLE_BITMAP_FORMAT = PixelFormats.Bgr32;

        private PlayerViewModel playerViewModel;
        private PlaybackModel playbackModel;
        private int currentFrame;

        private ManualResetEventSlim isVideoLoadedEvent;
        private ManualResetEventSlim isVideoPlayingEvent;

        // Filled by RetrieveFormatAndStreamInfo()
        private AVFormatContext* pFormatContext;

        // Filled by RetrieveVideoAndAudioStream()
        private AVStream* pVideoStream;
        private AVStream* pAudioStream;

        // Filled by RetrieveVideoCodecContextAndConvertContext()
        private AVCodecID videoCodecId;
        private AVCodecContext* pVideoCodecContext;
        private SwsContext* pConvertContext;
        private int width;
        private int height;
        private AVRational frameRate;

        // Filled by PrepareDecodeFrameAndPacket()
        private AVFrame* pDecodedVideoFrame;
        private AVPacket packet;

        // Filled by PrepareImageFrameAndBuffer()
        private int imageFrameBufferSize;

        public FFmpegDecoder(PlayerViewModel playerViewModel, PlaybackModel playbackModel)
        {
            this.playerViewModel = playerViewModel;
            this.playbackModel = playbackModel;
            this.isVideoLoadedEvent = new ManualResetEventSlim(false);
            this.isVideoPlayingEvent = new ManualResetEventSlim(false);

            pFormatContext = ffmpeg.avformat_alloc_context();

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
            else if ("Playing".Equals(e.PropertyName))
            {
                if (this.playbackModel.Playing)
                {
                    this.isVideoPlayingEvent.Set();
                }
                else
                {
                    this.isVideoPlayingEvent.Reset();
                }
            }
        }

        private void Load(string path)
        {
            currentFrame = 0;

            RetrieveFormatAndStreamInfo(path);
            RetrieveVideoAndAudioStream();

            // For video stream
            RetrieveVideoCodecContextAndConvertContext();
            FindAndOpenDecoder(this.pVideoCodecContext, this.videoCodecId);
            PrepareDecodedFrameAndPacket();
            PrepareImageFrameAndBuffer();
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
                while (true)
                {
                    this.isVideoLoadedEvent.Wait();

                    fixed (AVPacket* pPacket = &this.packet)
                    {
                        if (ReadFrame(pPacket))
                        {
                            if (this.packet.stream_index == this.pVideoStream->index)
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

            ffmpeg.avcodec_close(this.pVideoCodecContext);

            fixed (AVFormatContext** ppFormatContext = &this.pFormatContext)
            {
                ffmpeg.avformat_close_input(ppFormatContext);
            }
        }

        private void RetrieveFormatAndStreamInfo(string path)
        {
            fixed (AVFormatContext** ppFormatContext = &this.pFormatContext)
            {
                Util.AssertZero("FFmpeg: Cannot open file",
                    ffmpeg.avformat_open_input(ppFormatContext, path, null, null));
            }

            Util.AssertZero("FFmpeg: Cannot find stream info",
                ffmpeg.avformat_find_stream_info(this.pFormatContext, null));
        }

        private void RetrieveVideoAndAudioStream()
        {
            for (var i = 0; i < this.pFormatContext->nb_streams; i++)
            {
                var pStream = this.pFormatContext->streams[i];

                // TODO: Handle multiple video/audio stream
                switch (pStream->codec->codec_type)
                {
                    case AVMediaType.AVMEDIA_TYPE_VIDEO:
                        this.pVideoStream = pStream;
                        break;
                    case AVMediaType.AVMEDIA_TYPE_AUDIO:
                        this.pAudioStream = pStream;
                        break;
                }
            }

            if (this.pVideoStream == null)
            {
                Trace.WriteLine("FFmpeg: File has no video stream");
            }

            if (this.pAudioStream == null)
            {
                Trace.WriteLine("FFmpeg: File has no audio stream");
            }

            Util.AssertTrue("FFmpeg: Cannot find any video or audio stream",
                this.pVideoStream != null || this.pAudioStream != null);
        }

        private void RetrieveVideoCodecContextAndConvertContext()
        {
            if (this.pVideoStream == null)
            {
                return;
            }

            this.pVideoCodecContext = this.pVideoStream->codec;
            this.width = this.pVideoCodecContext->width;
            this.height = this.pVideoCodecContext->height;
            this.frameRate = this.pVideoCodecContext->framerate;
            var srcColorSpace = this.pVideoCodecContext->pix_fmt;

            this.videoCodecId = this.pVideoCodecContext->codec_id;
            this.pConvertContext = ffmpeg.sws_getContext(width, height, srcColorSpace,
                width, height, DISPLAY_COLOR_FORMAT, ffmpeg.SWS_FAST_BILINEAR, null, null, null);

            Util.AssertTrue("FFmpeg: Cannot initialize conversion context",
                this.pConvertContext != null);
        }

        private void FindAndOpenDecoder(AVCodecContext* pCodecContext, AVCodecID codeId)
        {
            var pCodec = ffmpeg.avcodec_find_decoder(codeId);

            Util.AssertTrue("FFmpeg: Cannot find decoder for video", pCodec != null);

            if ((pCodec->capabilities & ffmpeg.AV_CODEC_CAP_TRUNCATED) == ffmpeg.AV_CODEC_CAP_TRUNCATED)
            {
                pCodecContext->flags |= ffmpeg.AV_CODEC_CAP_TRUNCATED;
            }

            Util.AssertNonNegative("FFmpeg: Cannot open codec for " + codeId,
                ffmpeg.avcodec_open2(pCodecContext, pCodec, null));
        }

        private void PrepareDecodedFrameAndPacket()
        {
            this.pDecodedVideoFrame = ffmpeg.av_frame_alloc();
            this.packet = new AVPacket();

            fixed (AVPacket* pPacket = &this.packet)
            {
                ffmpeg.av_init_packet(pPacket);
            }
        }

        private void PrepareImageFrameAndBuffer()
        {
            this.imageFrameBufferSize = ffmpeg.avpicture_get_size(DISPLAY_COLOR_FORMAT, width, height);
            var numberOfBuffer = Math.Max(2, this.playerViewModel.MaxBufferSizeInMegabyte * 1024 * 1024 / this.imageFrameBufferSize);

            for (int i = 0; i < numberOfBuffer; i++)
            {
                var pImageFrame = ffmpeg.av_frame_alloc();
                var pImageBuffer = (sbyte*)ffmpeg.av_malloc((ulong)this.imageFrameBufferSize);
                var imageFramePtr = new IntPtr(pImageFrame);

                ffmpeg.avpicture_fill((AVPicture*)pImageFrame, pImageBuffer, DISPLAY_COLOR_FORMAT, width, height);
                this.playerViewModel.AvailableImageBufferPool.Add(imageFramePtr);
            }

            this.playerViewModel.VideoScreenBitmap = new WriteableBitmap(this.width, this.height, 72, 72, WRITEABLE_BITMAP_FORMAT, null);
        }

        private bool ReadFrame(AVPacket* pPacket)
        {
            currentFrame++;
            return ffmpeg.av_read_frame(pFormatContext, pPacket) >= 0;
        }

        private bool DecodeVideoFrame(AVPacket* pVideoPacket)
        {
            var gotPicture = 0;
            Util.AssertNonNegative("FFmpeg: Cannot decode video frame",
                ffmpeg.avcodec_decode_video2(pVideoCodecContext, pDecodedVideoFrame, &gotPicture, pVideoPacket));

            return gotPicture == 1;
        }

        private void ConvertFrameToImage(AVFrame* pImageFrame)
        {
            var src = &this.pDecodedVideoFrame->data0;
            var dest = &pImageFrame->data0;
            var srcStride = this.pDecodedVideoFrame->linesize;
            var destStride = pImageFrame->linesize;

            ffmpeg.sws_scale(pConvertContext, src, srcStride, 0, height, dest, destStride);
        }

        private void WriteImageToBuffer(AVFrame* pImageFrame)
        {
            var pImageBuffer = pImageFrame->data0;
            var imageBufferPtr = new IntPtr(pImageBuffer);
            var linesize = pImageFrame->linesize[0];

            this.playerViewModel.VideoScreenBitmap.Dispatcher.Invoke(() =>
            {
                this.playerViewModel.VideoScreenBitmap.Lock();
                CopyMemory(this.playerViewModel.VideoScreenBitmap.BackBuffer, imageBufferPtr, this.imageFrameBufferSize);
                this.playerViewModel.VideoScreenBitmap.AddDirtyRect(new Int32Rect(0, 0, this.width, this.height));
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
