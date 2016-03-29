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
            this.playViewModel.CurrentLyric = playbackModel.CurrentVideo.Lyric.GetCurrentLyric(currentTime);
        }

        void playbackModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("Playing".Equals(e.PropertyName))
            {
                if (this.playbackModel.Playing)
                {
                    this.timer.Change(0, interval);
                }
                else
                {
                    this.timer.Change(Timeout.Infinite, interval);
                    this.playViewModel.CurrentLyric = "";
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
