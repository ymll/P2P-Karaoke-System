using AviFile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Model
{
    public class PlaybackModel : AbstractNotifyPropertyChanged
    {
        private VideoDatabase.Video _currentVideo;
        public VideoDatabase.Video CurrentVideo
        {
            get { return _currentVideo; }
            set { SetField(ref _currentVideo, value, "CurrentVideo"); }
        }

        private long _currentTime = 0;
        public long CurrentTime
        {
            get { return _currentTime; }
            set { SetField(ref _currentTime, value, "CurrentTime"); }
        }

        private long _totalTime = 0;
        public long TotalTime
        {
            get { return _totalTime; }
            set { SetField(ref _totalTime, value, "TotalTime"); }
        }

        private bool _loaded = false;
        public bool Loaded
        {
            get { return _loaded; }
            set { SetField(ref _loaded, value, "Loaded"); }
        }

        private PlayState _state = PlayState.NotPlaying;
        public PlayState State
        {
            get { return _state; }
            set { SetField(ref _state, value, "State"); }
        }

        private int _volume = 255;
        public int Volume
        {
            get { return _volume; }
            set { SetField(ref _volume, value, "Volume"); }
        }

        public Avi.PCMWAVEFORMAT AudioWaveFormat { get; set; }
    }

    public enum PlayState
    {
        Playing, NotPlaying
    }
}
