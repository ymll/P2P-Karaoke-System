using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic
{
    public class LyricPlayer
    {
        private PlaybackModel playbackModel;
        private PlayerViewModel playViewModel;
        private Timer timer;
        private ManualResetEvent isTimerRunningEvent;
        private int currentTime;

        private const int interval = 100;

        public LyricPlayer(PlaybackModel playbackModel, PlayerViewModel playViewModel)
        {
            this.playbackModel = playbackModel;
            this.playViewModel = playViewModel;
            this.isTimerRunningEvent = new ManualResetEvent(true);
            this.timer = new Timer(TimerCallback, this.isTimerRunningEvent, Timeout.Infinite, interval);

            this.playbackModel.PropertyChanged += playbackModel_PropertyChanged;
        }

        private void TimerCallback(object state)
        {
            currentTime += interval;
            try
            {
                this.playViewModel.CurrentLyric = playbackModel.CurrentVideo.Lyric.GetCurrentLyric(currentTime);
            }
            catch (NullReferenceException) { }
        }

        void playbackModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("State".Equals(e.PropertyName))
            {
                switch (this.playbackModel.State)
                {
                    case PlayState.Playing:
                        this.timer.Change(0, interval);
                        break;
                    case PlayState.NotPlaying:
                        this.timer.Change(Timeout.Infinite, interval);
                        this.playViewModel.CurrentLyric = "";
                        break;
                }
            }
            else if ("Loaded".Equals(e.PropertyName))
            {
                if (this.playbackModel.Loaded)
                {
                    currentTime = 0;
                }
            }
        }

        public void Stop()
        {
            currentTime = 0;
            isTimerRunningEvent.Reset();
        }
    }
}
