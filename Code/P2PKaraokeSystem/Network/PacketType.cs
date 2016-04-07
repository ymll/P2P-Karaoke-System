using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    // TODO: Add more types if neccessary
    public enum PacketType
    {
        SEARCH_QUERY,
        SEARCH_RESULT,
        PLAY_REQUEST,
        MEDIA_INFO,
        VIDEO_STREAM,
        AUDIO_STREAM,
        SUBTITLE,
        UNDEFINED
    }
}
