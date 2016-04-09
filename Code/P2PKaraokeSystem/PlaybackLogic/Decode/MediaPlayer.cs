using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public class MediaPlayer
    {
        private VideoPlayer videoPlayer;

        public MediaPlayer(MediaDecodeInfo mediaDecodeInfo, PlayerViewModel playerViewModel, ManualResetEventSlim isVideoPlayingEvent)
        {
            videoPlayer = new VideoPlayer(mediaDecodeInfo.Video, playerViewModel, isVideoPlayingEvent);
        }

        public void StartAsync()
        {
            new Thread(() =>
            {
                while (true)
                {
                    Play();
                }
            }).Start();
        }

        private void Play()
        {
            videoPlayer.Play();
        }
    }
}
