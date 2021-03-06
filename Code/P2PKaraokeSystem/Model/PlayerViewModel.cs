﻿using FFmpeg.AutoGen;
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
        private WriteableBitmap _videoScreenBitmap;
        public WriteableBitmap VideoScreenBitmap
        {
            get { return _videoScreenBitmap; }
            set { SetField(ref _videoScreenBitmap, value, "VideoScreenBitmap"); }
        }

        private string _currentLyric = "";
        public string CurrentLyric
        {
            get { return _currentLyric; }
            set { SetField(ref _currentLyric, value, "CurrentLyric"); }
        }

        public bool IsQuit { get; set; }

        public BlockingCollection<Tuple<IntPtr, double>> PendingVideoFrames { get; private set; }

        public BlockingCollection<IntPtr> AvailableImageBufferPool { get; private set; }

        private const int DEFAULT_MAX_BUFFER_SIZE_IN_MEGABYTE = 200;
        public int MaxBufferSizeInMegabyte { get; private set; }

        public BlockingCollection<AVPacket> PendingAudioPackets { get; private set; }

        public BlockingCollection<AudioWaveData> PendingAudioWaveData { get; private set; }

        public PlayerViewModel()
        {
            this.PendingVideoFrames = new BlockingCollection<Tuple<IntPtr, double>>();
            this.MaxBufferSizeInMegabyte = DEFAULT_MAX_BUFFER_SIZE_IN_MEGABYTE;
            this.AvailableImageBufferPool = new BlockingCollection<IntPtr>(new ConcurrentBag<IntPtr>());

            this.PendingAudioPackets = new BlockingCollection<AVPacket>();
            this.PendingAudioWaveData = new BlockingCollection<AudioWaveData>();
        }
    }

    public struct AudioWaveData
    {
        public IntPtr data;
        public int start;
        public int size;
    }
}
