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
            player = new WaveOutPlayer(-1, fmt, 109200, 2, Filler);
        }

        private void Filler(IntPtr data, int size)
        {
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
                    IntPtr bufferPtr = new IntPtr(latestWaveData.data.ToInt32() + latestWaveData.start);
                    CopyMemory(data, bufferPtr, copySize);
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
                case "Loaded":
                    if (this.playbackModel.Loaded)
                    {
                        var aviHeaderParser = new P2PKaraokeSystem.PlaybackLogic.AviHeaderParser();
                        aviHeaderParser.LoadFile(this.playbackModel.CurrentVideo.FilePath);

                        AudioFrameReader frameReader = new AudioFrameReader();
                        frameReader.Load(aviHeaderParser.AudioHeaderReader);
                        frameReader.ReadFrameFully(aviHeaderParser.AudioHeaderReader);

                        AudioWaveData audioWaveData = new AudioWaveData();
                        audioWaveData.data = frameReader.FramePointer;
                        audioWaveData.start = 0;
                        audioWaveData.size = frameReader.FrameSize;
                        this.playerViewModel.PendingAudioWaveData.Add(audioWaveData);
                    }
                    break;

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

        [System.Runtime.InteropServices.DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);
    }
}
