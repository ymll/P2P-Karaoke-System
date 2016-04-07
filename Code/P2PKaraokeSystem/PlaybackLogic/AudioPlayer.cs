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

namespace P2PKaraokeSystem.PlaybackLogic
{
    public class AudioPlayer
    {
        public delegate void DelegateAudioFinished(Winmm.WOM_Messages msg);

        private IntPtr audioOut = new IntPtr();

        private PlaybackModel playbackModel;
        private PlayerViewModel playViewModel;
        private ManualResetEventSlim isAudioPlayingEvent;

        public AudioPlayer(PlaybackModel playbackModel, PlayerViewModel playViewModel)
        {
            this.playbackModel = playbackModel;
            this.playViewModel = playViewModel;
            this.isAudioPlayingEvent = new ManualResetEventSlim(false);

            this.playbackModel.PropertyChanged += playbackModel_PropertyChanged;
            this.playViewModel.PropertyChanged += playViewModel_PropertyChanged;
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
                        this.playViewModel.CurrentAudioFrame = new Tuple<IntPtr, int>(frameReader.FramePointer, frameReader.FrameSize);
                    }
                    break;

                case "CurrentVideo":
                case "State":
                    if (playbackModel.CurrentVideo != null && playbackModel.State == PlayState.Playing)
                    {
                        Winmm.waveOutRestart(audioOut);
                    }
                    else
                    {
                        Winmm.waveOutPause(audioOut);
                    }
                    break;

                case "Volume":
                    if (this.playbackModel.Volume == 0)
                    {
                        Winmm.waveOutSetVolume(audioOut, 0);
                    }
                    else
                    {
                        Winmm.waveOutSetVolume(audioOut, 0xFFFF);
                    }
                    break;
            }
        }

        private void playViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentAudioFrame":
                    WriteToStream(this.playViewModel.CurrentAudioFrame.Item1, this.playViewModel.CurrentAudioFrame.Item2);
                    break;
            }
        }

        private void OpenDevice(Avi.PCMWAVEFORMAT audioFormatInfo)
        {
            uint err = Winmm.waveOutOpen(ref audioOut, new IntPtr(Winmm.WAVE_MAPPER), ref audioFormatInfo, this.WaveOutProc, IntPtr.Zero, Winmm.WaveInOpenFlags.CALLBACK_FUNCTION);

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

            uint err = Winmm.waveOutPrepareHeader(audioOut, waveHdrPtr, Marshal.SizeOf(hdr));
            if (err != 0)
            {
                string errString = Winmm.waveOutGetErrorText(err);
                System.Diagnostics.Trace.WriteLine(errString);
            }

            err = Winmm.waveOutWrite(audioOut, ref hdr, (uint)Marshal.SizeOf(hdr));
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
