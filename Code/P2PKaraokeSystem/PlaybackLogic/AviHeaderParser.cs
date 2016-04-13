using AviFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic
{
    class AviHeaderParser
    {
        private int _aviFile;

        public AudioHeaderReader AudioHeaderReader { get; private set; }
        public VideoHeaderReader VideoHeaderReader { get; private set; }

        public void LoadFile(string filePath)
        {
            Avi.AVIFileInit();

            Util.AssertZero("Cannot open the file as AVI video",
                Avi.AVIFileOpen(ref _aviFile, filePath, Avi.OF_READWRITE, 0));

            AudioHeaderReader = new AudioHeaderReader();
            AudioHeaderReader.Load(_aviFile);

            VideoHeaderReader = new VideoHeaderReader();
            VideoHeaderReader.Load(_aviFile);
        }

        public void UnLoadFile()
        {
            if (_aviFile != 0)
            {
                Avi.AVIFileRelease(_aviFile);
            }
        }
    }
}
