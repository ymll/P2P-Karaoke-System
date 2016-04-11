using AviFile;
using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WaveLib;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public class AudioPlayer
    {
        private AudioDecodeInfo audioDecodeInfo;
        private PlayerViewModel playerViewModel;
        private PlaybackModel playbackModel;

        private WaveOutPlayer player;
        private AudioWaveData latestWaveData;

        public AudioPlayer(AudioDecodeInfo audioDecodeInfo, PlayerViewModel playerViewModel, PlaybackModel playbackModel)
        {
            this.audioDecodeInfo = audioDecodeInfo;
            this.playerViewModel = playerViewModel;
            this.playbackModel = playbackModel;

            this.playbackModel.PropertyChanged += playbackModel_PropertyChanged;

            WaveFormat fmt = new WaveFormat(44100, 16, 2);
            player = new WaveOutPlayer(-1, fmt, 109200, 10, Filler);
        }

        private void Filler(IntPtr data, int size)
        {
            int bufferCurrentIndex = 0;

            while (size > 0)
            {
                if (latestWaveData.size <= 0)
                {
                    latestWaveData = this.playerViewModel.PendingAudioWaveData.Take();
                }

                int bufferAvailable = latestWaveData.size - latestWaveData.start;
                int copySize = Math.Min(size, bufferAvailable);

                if (copySize > 0)
                {
                    IntPtr srcPtr = new IntPtr(latestWaveData.data.ToInt32() + latestWaveData.start);
                    IntPtr dstPtr = new IntPtr(data.ToInt32() + bufferCurrentIndex);
                    Util.CopyMemory(dstPtr, srcPtr, copySize);
                    bufferCurrentIndex += copySize;
                    latestWaveData.start += copySize;
                }

                if (latestWaveData.start >= latestWaveData.size)
                {
                    latestWaveData.size = latestWaveData.start = 0;
                }

                size -= copySize;
            }
        }

        private void playbackModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentVideo":
                case "State":
                    if (playbackModel.CurrentVideo != null && playbackModel.State == PlayState.Playing)
                    {
                        WaveNative.waveOutRestart(player.m_WaveOut);
                    }
                    else
                    {
                        WaveNative.waveOutPause(player.m_WaveOut);
                    }
                    break;

                case "Volume":
                    if (this.playbackModel.Volume == 0)
                    {
                        WaveNative.waveOutSetVolume(player.m_WaveOut, 0);
                    }
                    else
                    {
                        WaveNative.waveOutSetVolume(player.m_WaveOut, 0xFFFF);
                    }
                    break;
            }
        }
    }
}
