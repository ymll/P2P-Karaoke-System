using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinmmLib;

namespace P2PKaraokeSystem.PlaybackLogic
{
    public class PlayAudio
    {
        private IntPtr audioOut = new IntPtr();

        public void Play(AviFile.Avi.PCMWAVEFORMAT audioFormatInfo)
        {
            uint err = Winmm.waveOutOpen(ref audioOut, new IntPtr(Winmm.WAVE_MAPPER), ref audioFormatInfo, null, IntPtr.Zero, 0);

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
            hdr.dwBufferLength = (uint)(bufferSize / 4);
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

            System.Threading.Thread.Sleep(500);
        }
    }
}
