using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace P2PKaraokeSystem.Model
{
    public class PlayerViewModel : AbstractNotifyPropertyChanged
    {
        private bool _isPlaying = false;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { SetField(ref _isPlaying, value, "IsPlaying"); }
        }

        private WriteableBitmap _videoScreenBitmap;
        public WriteableBitmap VideoScreenBitmap
        {
            get { return _videoScreenBitmap; }
            set { SetField(ref _videoScreenBitmap, value, "VideoScreenBitmap"); }
        }
    }
}
