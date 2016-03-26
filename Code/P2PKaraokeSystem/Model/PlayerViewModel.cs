using System;
using System.Collections.Concurrent;
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

        public BlockingCollection<IntPtr> PendingVideoFrames { get; private set; }

        public BlockingCollection<IntPtr> AvailableImageBufferPool { get; private set; }

        private const int DEFAULT_NUMBER_OF_BUFFERED_IMAGE_FRAME = 100;
        public int NumberOfBufferedImageFrame { get; private set; }

        public PlayerViewModel()
        {
            this.PendingVideoFrames = new BlockingCollection<IntPtr>(new ConcurrentQueue<IntPtr>());
            this.NumberOfBufferedImageFrame = DEFAULT_NUMBER_OF_BUFFERED_IMAGE_FRAME;
            this.AvailableImageBufferPool = new BlockingCollection<IntPtr>(new ConcurrentBag<IntPtr>());
        }
    }
}
