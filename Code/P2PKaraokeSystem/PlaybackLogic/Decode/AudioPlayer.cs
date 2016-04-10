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
        private bool hasBuffer;
        private int current;

        public AudioPlayer(AudioDecodeInfo audioDecodeInfo, PlayerViewModel playerViewModel, PlaybackModel playbackModel)
        {
            this.audioDecodeInfo = audioDecodeInfo;
            this.playerViewModel = playerViewModel;
            this.playbackModel = playbackModel;

            this.playbackModel.PropertyChanged += playbackModel_PropertyChanged;
            this.playerViewModel.PropertyChanged += playerViewModel_PropertyChanged;

            WaveFormat fmt = new WaveFormat(44100, 16, 2);
            player = new WaveOutPlayer(-1, fmt, 109200, 2, Filler);
        }

        private void Filler(IntPtr data, int size)
        {
            if (hasBuffer)
            {
                IntPtr start = new IntPtr(this.playerViewModel.CurrentAudioFrame.Item1.ToInt32() + current);
                CopyMemory(data, start, size);
                current += size;
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

                        this.playerViewModel.CurrentAudioFrame = new Tuple<IntPtr, int>(frameReader.FramePointer, frameReader.FrameSize);
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

        private void playerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentAudioFrame":
                    this.hasBuffer = true;
                    this.current = 0;
                    break;
            }
        }

        [System.Runtime.InteropServices.DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);
    }
}
