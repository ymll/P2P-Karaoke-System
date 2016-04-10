using FFmpeg.AutoGen;
using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public unsafe class AudioDecoder
    {
        private AudioDecodeInfo audioDecodeInfo;
        private MediaDecodeInfo mediaDecodeInfo;
        private PlayerViewModel playerViewModel;

        public AudioDecoder(AudioDecodeInfo audioDecodeInfo, MediaDecodeInfo mediaDecodeInfo, PlayerViewModel playerViewModel)
        {
            this.audioDecodeInfo = audioDecodeInfo;
            this.mediaDecodeInfo = mediaDecodeInfo;
            this.playerViewModel = playerViewModel;
        }

        public void OnNewPacket(AVPacket* pAudioPacket)
        {
            AVPacket audioPacketCopy = new AVPacket();

            Util.AssertZero("Cannot setup new packet",
                ffmpeg.av_packet_ref(&audioPacketCopy, pAudioPacket));

            playerViewModel.PendingAudioFrames.Add(audioPacketCopy);
        }

        private AudioWaveData? Decode(out double pts)
        {
            AudioWaveData audioWaveData = new AudioWaveData();
            AVPacket* pPacket = null;
            int dataSize = 0;
            int n;

            pts = -1;

            for (; ; )
            {
                while (audioDecodeInfo.PacketSize > 0)
                {
                    int gotFrame = 0;
                    int len = ffmpeg.avcodec_decode_audio4(audioDecodeInfo.pCodecContext, audioDecodeInfo.pFrame, &gotFrame, pPacket);

                    if (len < 0)
                    {
                        audioDecodeInfo.PacketSize = 0;
                        break;
                    }

                    if (gotFrame != 0)
                    {
                        if (audioDecodeInfo.pFrame->format != (int)AVSampleFormat.AV_SAMPLE_FMT_S16)
                        {
                            return DecodeFrameFromPacket();
                        }
                        else
                        {
                            dataSize = ffmpeg.av_samples_get_buffer_size(
                                null,
                                audioDecodeInfo.NumOfChannels,
                                audioDecodeInfo.pFrame->nb_samples,
                                audioDecodeInfo.pCodecContext->sample_fmt,
                                1);

                            byte[] buffer = new byte[dataSize];
                            fixed (byte* bufferPtr = buffer)
                            {
                                CopyMemory((IntPtr)bufferPtr, (IntPtr)audioDecodeInfo.pFrame->data0, dataSize);
                                audioWaveData.data = (IntPtr)bufferPtr;
                            }
                        }
                    }

                    audioDecodeInfo.PacketData += len;
                    audioDecodeInfo.PacketSize -= len;

                    if (dataSize <= 0)
                    {
                        continue;
                    }

                    pts = audioDecodeInfo.Clock;
                    n = 2 * audioDecodeInfo.NumOfChannels;
                    audioDecodeInfo.Clock += (dataSize / (double)(n * audioDecodeInfo.pCodecContext->sample_rate));

                    audioWaveData.start = 0;
                    audioWaveData.size = dataSize;
                    return audioWaveData;
                }

                if (pPacket != null && pPacket->data != null)
                {
                    ffmpeg.av_free_packet(pPacket);
                }

                if (playerViewModel.IsQuit)
                {
                    return null;
                }

                AVPacket packet = playerViewModel.PendingAudioFrames.Take();
                pPacket = &packet;
                if (packet.data == MediaDecodeInfo.FlushPacket.data)
                {
                    ffmpeg.avcodec_flush_buffers(audioDecodeInfo.pCodecContext);
                    continue;
                }

                audioDecodeInfo.PacketData = packet.data;
                audioDecodeInfo.PacketSize = packet.size;

                if ((ulong)packet.pts != ffmpeg.AV_NOPTS_VALUE)
                {
                    audioDecodeInfo.Clock = q2d(audioDecodeInfo.pStream->time_base) * packet.pts;
                }
            }
        }

        private double q2d(AVRational a)
        {
            return a.num / (double)a.den;
        }

        private AudioWaveData DecodeFrameFromPacket()
        {
            AudioWaveData audioWaveData = new AudioWaveData();

            long src_ch_layout, dst_ch_layout;
            long src_rate, dst_rate;
            sbyte** src_data = null;
            sbyte** dst_data = null;
            int src_nb_channels = 0, dst_nb_channels = 0;
            int src_linesize, dst_linesize;
            long src_nb_samples, dst_nb_samples, max_dst_nb_samples;
            AVSampleFormat src_sample_fmt, dst_sample_fmt;
            int dst_bufsize;

            AVFrame* decodedFrame = audioDecodeInfo.pFrame;
            src_nb_samples = decodedFrame->nb_samples;
            src_linesize = (int)decodedFrame->linesize;
            src_data = &decodedFrame->data0;

            if (decodedFrame->channel_layout == 0)
            {
                decodedFrame->channel_layout = (ulong)ffmpeg.av_get_default_channel_layout(decodedFrame->channels);
            }

            dst_rate = src_rate = decodedFrame->sample_rate;
            dst_ch_layout = src_ch_layout = (long)decodedFrame->channel_layout;
            src_sample_fmt = (AVSampleFormat)decodedFrame->format;
            dst_sample_fmt = AVSampleFormat.AV_SAMPLE_FMT_S16;

            SwrContext* pResampleContext = audioDecodeInfo.pResampleContext;
            ffmpeg.av_opt_set_int((void*)pResampleContext, "in_channel_layout", src_ch_layout, 0);
            ffmpeg.av_opt_set_int((void*)pResampleContext, "out_channel_layout", dst_ch_layout, 0);
            ffmpeg.av_opt_set_int((void*)pResampleContext, "in_sample_rate", src_rate, 0);
            ffmpeg.av_opt_set_int((void*)pResampleContext, "out_sample_rate", dst_rate, 0);
            ffmpeg.av_opt_set_sample_fmt((void*)pResampleContext, "in_sample_fmt", src_sample_fmt, 0);
            ffmpeg.av_opt_set_sample_fmt((void*)pResampleContext, "out_sample_fmt", dst_sample_fmt, 0);

            Util.AssertNonNegative("Cannot initialize resampling context", ffmpeg.swr_init(pResampleContext));

            // Allocate sample buffers
            src_nb_channels = ffmpeg.av_get_channel_layout_nb_channels((ulong)src_ch_layout);
            Util.AssertNonNegative("Cannot allocate source samples",
                ffmpeg.av_samples_alloc_array_and_samples(&src_data, &src_linesize, src_nb_channels, (int)src_nb_samples, src_sample_fmt, 0));

            max_dst_nb_samples = dst_nb_samples = ffmpeg.av_rescale_rnd((long)src_nb_samples, dst_rate, src_rate, AVRounding.AV_ROUND_UP);
            dst_nb_channels = ffmpeg.av_get_channel_layout_nb_channels((ulong)dst_ch_layout);
            Util.AssertNonNegative("Cannot allocate destination samples",
                ffmpeg.av_samples_alloc_array_and_samples(&dst_data, &dst_linesize, dst_nb_channels, (int)dst_nb_samples, dst_sample_fmt, 0));

            dst_nb_samples = ffmpeg.av_rescale_rnd(ffmpeg.swr_get_delay(pResampleContext, src_rate) + src_nb_samples, dst_rate, src_rate, AVRounding.AV_ROUND_UP);
            Util.AssertNonNegative("Cannot convert audio",
                //ffmpeg.swr_convert(pResampleContext, dst_data, (int)dst_nb_samples, src_data, (int)src_nb_samples));
            ffmpeg.swr_convert(pResampleContext, dst_data, (int)dst_nb_samples, &decodedFrame->data0, (int)src_nb_samples));

            dst_bufsize = ffmpeg.av_samples_get_buffer_size(&dst_linesize, dst_nb_channels, (int)dst_nb_samples, dst_sample_fmt, 1);
            Util.AssertNonNegative("Cannot get sample buffer size", dst_bufsize);

            byte[] buffer = new byte[dst_bufsize];
            fixed (byte* pBuffer = buffer)
            {
                CopyMemory((IntPtr)pBuffer, (IntPtr)dst_data[0], dst_bufsize);
                audioWaveData.data = (IntPtr)pBuffer;
                audioWaveData.start = 0;
                audioWaveData.size = dst_bufsize;
            }

            if (src_data != null)
            {
                ffmpeg.av_freep(&src_data[0]);
            }
            ffmpeg.av_freep(&src_data);

            if (dst_data != null)
            {
                ffmpeg.av_freep(&dst_data[0]);
            }
            ffmpeg.av_freep(&dst_data);

            return audioWaveData;
        }

        public void Start()
        {
            for (; ; )
            {
                double pts = 0;
                AudioWaveData? audioWaveData = Decode(out pts);

                if (audioWaveData.HasValue)
                {
                    playerViewModel.PendingAudioWaveData.Add(audioWaveData.Value);
                }
            }
        }

        private int SyncAudio(short* samples, int samplesSize, double pts)
        {
            int n = 2 * audioDecodeInfo.pCodecContext->channels;
            double ref_clock;

            if (mediaDecodeInfo.Sync != MediaDecodeInfo.SyncType.AUDIO)
            {
                double diff, avg_diff;
                int wanted_size, min_size, max_size;

                ref_clock = mediaDecodeInfo.GetMasterClock();
                diff = audioDecodeInfo.GetClock() - ref_clock;

                if (diff < MediaDecodeInfo.AV_NOSYNC_THRESHOLD)
                {
                    audioDecodeInfo.DiffCum = diff + audioDecodeInfo.DiffAvgCoef * audioDecodeInfo.DiffCum;

                    if (audioDecodeInfo.DiffAvgCount < AudioDecodeInfo.AUDIO_DIFF_AVG_NB)
                    {
                        audioDecodeInfo.DiffAvgCount++;
                    }
                    else
                    {
                        avg_diff = audioDecodeInfo.DiffCum * (1.0 - audioDecodeInfo.DiffAvgCoef);

                        if (Math.Abs(avg_diff) >= audioDecodeInfo.DiffThreshold)
                        {
                            wanted_size = samplesSize + ((int)(diff * audioDecodeInfo.pCodecContext->sample_rate) * n);
                            min_size = samplesSize * ((100 - AudioDecodeInfo.SAMPLE_CORRECTION_PERCENT_MAX) / 100);
                            max_size = samplesSize * ((100 + AudioDecodeInfo.SAMPLE_CORRECTION_PERCENT_MAX) / 100);

                            if (wanted_size < min_size)
                            {
                                wanted_size = min_size;
                            }
                            else if (wanted_size > max_size)
                            {
                                wanted_size = max_size;
                            }

                            if (wanted_size < samplesSize)
                            {
                                samplesSize = wanted_size;
                            }
                            else if (wanted_size > samplesSize)
                            {
                                byte* samples_end;
                                byte* q;
                                int nb = (samplesSize - wanted_size);
                                samples_end = (byte*)samples + samplesSize - n;
                                q = samples_end + n;
                                while (nb > 0)
                                {
                                    CopyMemory((IntPtr)q, (IntPtr)samples_end, n);
                                    q += n;
                                    nb -= n;
                                }
                                samplesSize = wanted_size;
                            }
                        }
                    }
                }
                else
                {
                    audioDecodeInfo.DiffAvgCount = 0;
                    audioDecodeInfo.DiffCum = 0;
                }
            }

            return samplesSize;
        }

        [System.Runtime.InteropServices.DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);
    }
}
