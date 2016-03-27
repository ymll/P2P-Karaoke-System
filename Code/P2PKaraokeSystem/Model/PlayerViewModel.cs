using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
