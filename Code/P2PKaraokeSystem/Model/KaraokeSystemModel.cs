using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Model
{
    public class KaraokeSystemModel
    {
        public PlaybackModel Playback { get; set; }
        public PlayerViewModel View { get; set; }

        public KaraokeSystemModel()
        {
            Playback = new PlaybackModel();
            View = new PlayerViewModel();
        }
    }
}
