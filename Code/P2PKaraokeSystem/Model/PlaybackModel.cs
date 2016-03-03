using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Model
{
    public class PlaybackModel : AbstractNotifyPropertyChanged
    {
        private const String DEFAULT_FILE_PATH = "[File not loaded]";
        private String _filePath = DEFAULT_FILE_PATH;
        public String FilePath
        {
            get { return _filePath; }
            set { SetField(ref _filePath, value, "FilePath"); }
        }

        private long _currentTime = 0;
        public long CurrentTime
        {
            get { return _currentTime; }
            set { SetField(ref _currentTime, value, "CurrentTime"); }
        }

        private long _totalTime = 0;
        public long TotalTime
        {
            get { return _totalTime; }
            set { SetField(ref _totalTime, value, "TotalTime"); }
        }
    }
}
