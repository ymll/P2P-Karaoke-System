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
        private DelegateAudioFinished finishCallback;

        private PlaybackModel playbackModel;
        private ManualResetEventSlim isAudioPlayingEvent;

        public AudioPlayer(PlaybackModel playbackModel)
        {
            this.playbackModel = playbackModel;
            this.isAudioPlayingEvent = new ManualResetEventSlim(false);

            this.playbackModel.PropertyChanged += playbackModel_PropertyChanged;
        }

        private void playbackModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentVideo":
                case "Playing":
                    if (playbackModel.CurrentVideo != null && playbackModel.Playing)
                    {
                        Winmm.waveOutRestart(audioOut);
                    }
                    else
                    {
                        Winmm.waveOutPause(audioOut);
                    }
                    break;
            }
        }

        public void OpenDevice(AviFile.Avi.PCMWAVEFORMAT audioFormatInfo, DelegateAudioFinished finishCallback)
        {
            this.finishCallback = finishCallback;
            uint err = Winmm.waveOutOpen(ref audioOut, new IntPtr(Winmm.WAVE_MAPPER), ref audioFormatInfo, this.WaveOutProc, IntPtr.Zero, Winmm.WaveInOpenFlags.CALLBACK_FUNCTION);

            if (err != 0)
            {
                string errString = Winmm.waveOutGetErrorText(err);
                System.Diagnostics.Trace.WriteLine(errString);
            }
        }

        public void WriteToStream(IntPtr buffer, int bufferSize)
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
            finishCallback(msg);
        }
    }
}
