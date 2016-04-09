using AviFile;
using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinmmLib;

namespace P2PKaraokeSystem.PlaybackLogic.Decode
{
    public class AudioPlayer
    {
        private AudioDecodeInfo audioDecodeInfo;
        private PlayerViewModel playerViewModel;
        private PlaybackModel playbackModel;

        public AudioPlayer(AudioDecodeInfo audioDecodeInfo, PlayerViewModel playerViewModel, PlaybackModel playbackModel)
        {
            this.audioDecodeInfo = audioDecodeInfo;
            this.playerViewModel = playerViewModel;
            this.playbackModel = playbackModel;

            this.playbackModel.PropertyChanged += playbackModel_PropertyChanged;
            this.playerViewModel.PropertyChanged += playerViewModel_PropertyChanged;
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

                        OpenDevice(aviHeaderParser.AudioHeaderReader.FormatInfo);
                        this.playerViewModel.CurrentAudioFrame = new Tuple<IntPtr, int>(frameReader.FramePointer, frameReader.FrameSize);
                    }
                    break;

                case "CurrentVideo":
                case "State":
                    if (playbackModel.CurrentVideo != null && playbackModel.State == PlayState.Playing)
                    {
                        Winmm.waveOutRestart(audioDecodeInfo.AudioOut);
                    }
                    else
                    {
                        Winmm.waveOutPause(audioDecodeInfo.AudioOut);
                    }
                    break;

                case "Volume":
                    if (this.playbackModel.Volume == 0)
                    {
                        Winmm.waveOutSetVolume(audioDecodeInfo.AudioOut, 0);
                    }
                    else
                    {
                        Winmm.waveOutSetVolume(audioDecodeInfo.AudioOut, 0xFFFF);
                    }
                    break;
            }
        }

        private void playerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentAudioFrame":
                    WriteToStream(this.playerViewModel.CurrentAudioFrame.Item1, this.playerViewModel.CurrentAudioFrame.Item2);
                    break;
            }
        }

        private void OpenDevice(Avi.PCMWAVEFORMAT audioFormatInfo)
        {
            uint err = Winmm.waveOutOpen(ref audioDecodeInfo.AudioOut, new IntPtr(Winmm.WAVE_MAPPER), ref audioFormatInfo, this.WaveOutProc, IntPtr.Zero, Winmm.WaveInOpenFlags.CALLBACK_FUNCTION);

            if (err != 0)
            {
                string errString = Winmm.waveOutGetErrorText(err);
                System.Diagnostics.Trace.WriteLine(errString);
            }
        }

        private void WriteToStream(IntPtr buffer, int bufferSize)
        {
            Winmm.WAVEHDR hdr = new Winmm.WAVEHDR();
            hdr.lpData = buffer;
            hdr.dwBufferLength = (uint)(bufferSize);
            hdr.dwFlags = Winmm.WaveHdrFlags.WHDR_PREPARED;

            var waveHdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(hdr));
            Marshal.StructureToPtr(hdr, waveHdrPtr, true);

            uint err = Winmm.waveOutPrepareHeader(audioDecodeInfo.AudioOut, waveHdrPtr, Marshal.SizeOf(hdr));
            if (err != 0)
            {
                string errString = Winmm.waveOutGetErrorText(err);
                System.Diagnostics.Trace.WriteLine(errString);
            }

            err = Winmm.waveOutWrite(audioDecodeInfo.AudioOut, ref hdr, (uint)Marshal.SizeOf(hdr));
            if (err != 0)
            {
                string errString = Winmm.waveOutGetErrorText(err);
                System.Diagnostics.Trace.WriteLine(errString);
            }
            System.Threading.Thread.Sleep(5);
        }

        private void WaveOutProc(IntPtr hWaveOut, Winmm.WOM_Messages msg, IntPtr dwInstance, ref Winmm.WAVEHDR wavehdr, IntPtr lParam)
        {

        }
    }
}
