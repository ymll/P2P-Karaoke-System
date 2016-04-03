using P2PKaraokeSystem.Model;
using P2PKaraokeSystem.PlaybackLogic;
using P2PKaraokeSystem.PlaybackLogic.Native.FFmpeg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace P2PKaraokeSystem.View
{

    public partial class MainUI : Window
    {
        private KaraokeSystemModel _karaokeSystemModel;

        public MainUI()
        {
            InitializeComponent();
            FFmpegLoader.LoadFFmpeg();

            this._karaokeSystemModel = (KaraokeSystemModel)this.DataContext;
            new LyricPlayer(this._karaokeSystemModel.Playback, this._karaokeSystemModel.View);
            new FFmpegDecoder(this._karaokeSystemModel.View, this._karaokeSystemModel.Playback);
        }

        private void playImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this._karaokeSystemModel.Playback.Playing = !this._karaokeSystemModel.Playback.Playing;
        }

        private void soundImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this._karaokeSystemModel.Playback.Volume == 0)
            {
                this._karaokeSystemModel.Playback.Volume = 255;
            }
            else
            {
                this._karaokeSystemModel.Playback.Volume = 0;
            }
        }

        private void UpdatePlayState(bool isPlay)
        {
            this._karaokeSystemModel.Playback.Playing = isPlay;

            if (this._karaokeSystemModel.Playback.Playing)
            {
                var aviHeaderParser = new P2PKaraokeSystem.PlaybackLogic.AviHeaderParser();
                aviHeaderParser.LoadFile("Z:\\Users\\sonia\\note\\year4\\sem2\\CSCI3280\\Project\\Code\\P2PKaraokeSystem\\VideoDatabase\\Video\\only_time.avi");

                AudioFrameReader frameReader = new AudioFrameReader();
                frameReader.Load(aviHeaderParser.AudioHeaderReader);
                frameReader.ReadFrameFully(aviHeaderParser.AudioHeaderReader);

                AudioPlayer audioPlayer = new AudioPlayer(this._karaokeSystemModel.Playback);

                audioPlayer.OpenDevice(aviHeaderParser.AudioHeaderReader.FormatInfo, delegate { });
                audioPlayer.WriteToStream(frameReader.FramePointer, frameReader.FrameSize);
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this._karaokeSystemModel.Playback.CurrentVideo = ((sender as ListView).SelectedItem as Model.VideoDatabase.Video);
            UpdatePlayState(true);
        }
    }
}
