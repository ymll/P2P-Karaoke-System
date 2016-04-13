using AviFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic
{
    class AudioFrameReader
    {
        public IntPtr FramePointer { get; private set; }
        public int FrameSize { get; private set; }

        public void Load(AudioHeaderReader header)
        {
            FramePointer = Marshal.AllocHGlobal(header.AudioStreamLength);
        }

        public bool ReadFrameFully(AudioHeaderReader header)
        {
            return ReadFrame(header, TimeSpan.Zero);
        }

        public bool ReadFrame(AudioHeaderReader header, TimeSpan timeSpan)
        {
            int startByteIndex = (int)(header.AudioStreamLength * timeSpan.TotalSeconds);
            int endByteIndex = startByteIndex + header.AudioStreamLength;

            System.Diagnostics.Trace.WriteLine(string.Format("Total Seconds: {0}", timeSpan.TotalSeconds));

            try
            {
                ReadFrame(header, startByteIndex, endByteIndex);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ReadFrame(AudioHeaderReader header, int startByteIndex, int endByteIndex)
        {
            int numOfByteOfFrame = endByteIndex - startByteIndex;
            startByteIndex += header.FirstAudioFrame;
            endByteIndex += header.FirstAudioFrame;
            System.Diagnostics.Trace.WriteLine(string.Format("{0} ~ {1} [{2}~{3}]", startByteIndex, endByteIndex, header.FirstAudioFrame, header.FirstAudioFrame + header.FrameCount));

            Util.AssertZero("Cannot read audio stream",
                Avi.AVIStreamRead(header.Stream, startByteIndex, endByteIndex, this.FramePointer, numOfByteOfFrame, 0, 0));

            FrameSize = numOfByteOfFrame;
        }

        public void UnLoad()
        {
            Marshal.FreeHGlobal(this.FramePointer);
        }
    }
}
