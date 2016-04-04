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
            new AudioPlayer(this._karaokeSystemModel.Playback, this._karaokeSystemModel.View);
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
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this._karaokeSystemModel.Playback.CurrentVideo = ((sender as ListView).SelectedItem as Model.VideoDatabase.Video);
            UpdatePlayState(true);
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                this._karaokeSystemModel.VideoDatabase.LoadForSearch("Z:\\Users\\sonia\\note\\year4\\sem2\\CSCI3280\\Project\\Code\\P2PKaraokeSystem\\VideoDatabase\\db.csv",searchBox.Text);
            }
        }
    }
}
