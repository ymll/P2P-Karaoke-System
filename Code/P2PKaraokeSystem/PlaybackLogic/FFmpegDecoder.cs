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
        private Thread decodeThread;
        private int currentFrame;

        // Filled by RetrieveFormatAndStreamInfo()
        private AVFormatContext* pFormatContext;

        // Filled by RetrieveVideoAndAudioStream()
        private AVStream* pVideoStream;
        private AVStream* pAudioStream;

        // Filled by RetrieveVideoCodecContextAndConvertContext()
        private AVCodecID codecId;
        private AVCodecContext* pVideoCodecContext;
        private SwsContext* pConvertContext;
        private int width;
        private int height;
        private AVRational frameRate;

        // Filled by PrepareDecodeFrameAndPacket()
        private AVFrame* pDecodedVideoFrame;
        private AVPacket videoPacket;

        // Filled by PrepareImageFrameAndBuffer()
        private int imageFrameBufferSize;

        public FFmpegDecoder(PlayerViewModel playerViewModel)
        {
            this.playerViewModel = playerViewModel;
            this.decodeThread = new Thread(this.DecodeFunc);
            pFormatContext = ffmpeg.avformat_alloc_context();

            this.StartPlaybackThread();
        }

        public void Load(string path)
        {
            currentFrame = 0;

            RetrieveFormatAndStreamInfo(path);
            RetrieveVideoAndAudioStream();

            // For video stream
            RetrieveVideoCodecContextAndConvertContext();
            FindAndOpenVideoDecoder();
            PrepareDecodedFrameAndPacket();
            PrepareImageFrameAndBuffer();
        }

        public void StartDecode()
        {
            if (decodeThread.ThreadState != System.Threading.ThreadState.Suspended)
            {
                decodeThread.Start();
            }
        }

        private void StartPlaybackThread()
        {
            // Playback thread
            new Thread(() =>
            {
                while (true)
                {
                    IntPtr imageFramePtr = this.playerViewModel.PendingVideoFrames.Take();
                    var pImageFrame = (AVFrame*)imageFramePtr.ToPointer();

                    WriteImageToBuffer(pImageFrame);
                    this.playerViewModel.AvailableImageBufferPool.Add(imageFramePtr);

                    int sleepTime = (int)(frameRate.den * 1000.0 / frameRate.num);
                    Thread.Sleep(sleepTime);
                }
            }).Start();
        }

        private void DecodeFunc()
        {
            fixed (AVPacket* pVideoPacket = &this.videoPacket)
            {
                IntPtr imageFramePtr = this.playerViewModel.AvailableImageBufferPool.Take();

                while (ReadFrame(pVideoPacket))
                {
                    if (IsVideoFrame(pVideoPacket) && DecodeVideoFrame(pVideoPacket))
                    {
                        var pImageFrame = (AVFrame*)imageFramePtr.ToPointer();

                        ConvertFrameToImage(pImageFrame);

                        if (IS_FRAME_SAVE_TO_FILE && currentFrame % 20 == 0)
                        {
                            SaveBufferToFile();
                        }

                        this.playerViewModel.PendingVideoFrames.Add(imageFramePtr);
                        imageFramePtr = this.playerViewModel.AvailableImageBufferPool.Take();
                    }
                }
            }
        }

        public void UnLoad()
        {

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

            this.codecId = this.pVideoCodecContext->codec_id;
            this.pConvertContext = ffmpeg.sws_getContext(width, height, srcColorSpace,
                width, height, DISPLAY_COLOR_FORMAT, ffmpeg.SWS_FAST_BILINEAR, null, null, null);

            Util.AssertTrue("FFmpeg: Cannot initialize conversion context",
                this.pConvertContext != null);
        }

        private void FindAndOpenVideoDecoder()
        {
            var pCodec = ffmpeg.avcodec_find_decoder(this.codecId);

            Util.AssertTrue("FFmpeg: Cannot find decoder for video", pCodec != null);

            if ((pCodec->capabilities & ffmpeg.AV_CODEC_CAP_TRUNCATED) == ffmpeg.AV_CODEC_CAP_TRUNCATED)
            {
                this.pVideoCodecContext->flags |= ffmpeg.AV_CODEC_CAP_TRUNCATED;
            }

            Util.AssertNonNegative("FFmpeg: Cannot open codec for video",
                ffmpeg.avcodec_open2(pVideoCodecContext, pCodec, null));
        }

        private void PrepareDecodedFrameAndPacket()
        {
            this.pDecodedVideoFrame = ffmpeg.av_frame_alloc();
            this.videoPacket = new AVPacket();

            fixed (AVPacket* pVideoPacket = &this.videoPacket)
            {
                ffmpeg.av_init_packet(pVideoPacket);
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

        private bool ReadFrame(AVPacket* pVideoPacket)
        {
            currentFrame++;
            return ffmpeg.av_read_frame(pFormatContext, pVideoPacket) >= 0;
        }

        private bool IsVideoFrame(AVPacket* pPacket)
        {
            return pPacket->stream_index == this.pVideoStream->index;
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
