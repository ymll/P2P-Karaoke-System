using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public abstract unsafe class StreamLoader
    {
        private StreamDecodeInfo decodeInfo;

        public StreamLoader(StreamDecodeInfo decodeInfo)
        {
            this.decodeInfo = decodeInfo;
        }

        public abstract void Load();

        protected void FindAndOpenDecoder()
        {
            decodeInfo.pCodecContext = decodeInfo.pStream->codec;
            var pCodec = ffmpeg.avcodec_find_decoder(decodeInfo.pCodecContext->codec_id);

            Util.AssertTrue("FFmpeg: Cannot find decoder for video", pCodec != null);

            if ((pCodec->capabilities & ffmpeg.AV_CODEC_CAP_TRUNCATED) == ffmpeg.AV_CODEC_CAP_TRUNCATED)
            {
                decodeInfo.pCodecContext->flags |= ffmpeg.AV_CODEC_CAP_TRUNCATED;
            }

            Util.AssertNonNegative("FFmpeg: Cannot open codec for " + decodeInfo.pCodecContext->codec_id,
                ffmpeg.avcodec_open2(decodeInfo.pCodecContext, pCodec, null));
        }
    }
}
