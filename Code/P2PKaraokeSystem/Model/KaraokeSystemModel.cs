using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Model
{
    public class KaraokeSystemModel
    {
        public PlaybackModel Playback { get; private set; }
        public PlayerViewModel View { get; private set; }
        public VideoDatabase VideoDatabase { get; private set; }

        public KaraokeSystemModel()
        {
            this.Playback = new PlaybackModel();
            this.View = new PlayerViewModel();
            this.VideoDatabase = new VideoDatabase();
        }
    }
}
