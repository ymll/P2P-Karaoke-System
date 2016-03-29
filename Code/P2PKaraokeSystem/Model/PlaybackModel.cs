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

        private bool _playing = false;
        public bool Playing
        {
            get { return _playing; }
            set { SetField(ref _playing, value, "Playing"); }
        }
    }
}
