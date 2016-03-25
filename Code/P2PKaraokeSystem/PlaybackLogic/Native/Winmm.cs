using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinmmLib
{
    public class Winmm
    {
        public const int WAVE_MAPPER = -1;

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutOpen(ref IntPtr hWaveOut, IntPtr uDeviceID, ref AviFile.Avi.PCMWAVEFORMAT lpFormat, DelegateWaveOutProc dwCallback, IntPtr dwInstance, WaveInOpenFlags dwFlags);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutPrepareHeader(IntPtr hWaveOut, IntPtr pwh, int uSize);
        // Need to use IntPtr as WAVEHEADER struct must be in a fixed memory location. See Tips & Tricks below

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutWrite(IntPtr hwo, ref WAVEHDR pwh, uint cbwh);

        public delegate void DelegateWaveOutProc(IntPtr hWaveOut, WOM_Messages msg, IntPtr dwInstance, ref WAVEHDR wavehdr, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WAVEHDR
        {
            public IntPtr lpData; // pointer to locked data buffer
            public uint dwBufferLength; // length of data buffer
            public uint dwBytesRecorded; // used for input only
            public IntPtr dwUser; // for client's use
            public WaveHdrFlags dwFlags; // assorted flags (see defines)
            public uint dwLoops; // loop control counter
            public IntPtr lpNext; // PWaveHdr, reserved for driver
            public IntPtr reserved; // reserved for driver
        }

        [Flags]
        public enum WaveHdrFlags : uint
        {
            WHDR_DONE = 1,
            WHDR_PREPARED = 2,
            WHDR_BEGINLOOP = 4,
            WHDR_ENDLOOP = 8,
            WHDR_INQUEUE = 16
        }

        [Flags]
        public enum WaveInOpenFlags : uint
        {
            CALLBACK_NULL = 0,
            CALLBACK_FUNCTION = 0x30000,
            CALLBACK_EVENT = 0x50000,
            CALLBACK_WINDOW = 0x10000,
            CALLBACK_THREAD = 0x20000,
            WAVE_FORMAT_QUERY = 1,
            WAVE_MAPPED = 4,
            WAVE_FORMAT_DIRECT = 8
        }

        public enum WOM_Messages : int
        {
            OPEN = 0x03BB,
            CLOSE = 0x03BC,
            DONE = 0x03BD
        }

        private const int MAXERRORLENGTH = 256;

        /// <summary>
        /// This function gets a string describing the error returned from one of the wave functions.
        /// </summary>
        /// <param name="mmrError">The error code</param>
        /// <param name="pszText">String returning the text description</param>
        /// <param name="cchText">The size of the string</param>
        /// <returns>NOERROR success, otherwise fail.</returns>
        [DllImport("winmm.dll", EntryPoint = "waveOutGetErrorText", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint waveOutGetErrorText(uint mmrError, StringBuilder pszText, uint cchText);

        /// <summary>
        /// This function gets a string describing the error returned from one of the wave functions.
        /// </summary>
        /// <param name="mmrError">The error code</param>
        /// <returns>The description</returns>
        public static string waveOutGetErrorText(uint mmrError)
        {
            StringBuilder message = new StringBuilder(MAXERRORLENGTH);
            uint errorResult = waveOutGetErrorText(mmrError, message, (uint)message.Capacity);
            if (errorResult == 0)
            {
                return message.ToString();
            }
            else
            {
                return "waveOutGetErrorText failed.";
            }
        }
    }
}
