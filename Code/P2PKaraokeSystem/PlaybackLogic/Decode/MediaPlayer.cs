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
        private AudioPlayer audioPlayer;

        public MediaPlayer(MediaDecodeInfo mediaDecodeInfo, PlayerViewModel playerViewModel, PlaybackModel playbackModel, ManualResetEventSlim isVideoPlayingEvent)
        {
            videoPlayer = new VideoPlayer(mediaDecodeInfo.Video, playerViewModel, isVideoPlayingEvent);
            audioPlayer = new AudioPlayer(mediaDecodeInfo.Audio, playerViewModel, playbackModel);
        }

        public void StartAsync()
        {
            new Thread(() =>
            {
                while (true)
                {
                    videoPlayer.Play();
                }
            }).Start();
        }
    }
}
