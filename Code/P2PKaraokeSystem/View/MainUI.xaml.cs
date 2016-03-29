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
        private FFmpegDecoder decoder;

        bool soundOn = true;

        public System.Diagnostics.Stopwatch musicTime;
        public P2PKaraokeSystem.Model.VideoDatabase.Lyric currentLyricFile;

        public MainUI()
        {
            InitializeComponent();
            FFmpegLoader.LoadFFmpeg();

            this._karaokeSystemModel = (KaraokeSystemModel)this.DataContext;
            this.decoder = new FFmpegDecoder(this._karaokeSystemModel.View, this._karaokeSystemModel.Playback);
            currentLyricFile = new P2PKaraokeSystem.Model.VideoDatabase.Lyric("Z:\\Code\\P2PKaraokeSystem\\VideoDatabase\\Lyrics\\Enya - Only Time.lrc");
        }

        //Set Image Source for an image throught Uri
        private void SetImage(Image imageName, string path)
        {
            imageName.BeginInit();
            imageName.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            imageName.EndInit();

        }

        private void playImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this._karaokeSystemModel.Playback.Playing = !this._karaokeSystemModel.Playback.Playing;
        }

        //Backward Button Enter
        private void backwardBtn_MouseEnter(object sender, EventArgs e)
        {
            SetImage(backwardImg, "pack://application:,,,/View/UIMaterial/Image/backward_blue.png");
        }

        //Backward Button Mouse Leave
        private void backwardBtn_MouseLeave(object sender, EventArgs e)
        {
            SetImage(backwardImg, "pack://application:,,,/View/UIMaterial/Image/backward.png");
        }

        //Forward Button Enter
        private void fastforwardBtn_MouseEnter(object sender, EventArgs e)
        {
            SetImage(fastforwardImg, "pack://application:,,,/View/UIMaterial/Image/fastforward_blue.png");
        }

        //Forward Button Mouse Leave
        private void fastforwardBtn_MouseLeave(object sender, EventArgs e)
        {
            SetImage(fastforwardImg, "pack://application:,,,/View/UIMaterial/Image/fastforward.png");
        }

        //Sound Button Enter
        private void soundBtn_MouseEnter(object sender, EventArgs e)
        {
            if (soundOn)
                SetImage(soundImg, "pack://application:,,,/View/UIMaterial/Image/volumeup_blue.png");
            else
                SetImage(soundImg, "pack://application:,,,/View/UIMaterial/Image/volumeoff_blue.png");
        }

        //Sound Button Mouse Leave
        private void soundBtn_MouseLeave(object sender, EventArgs e)
        {
            if (soundOn)
                SetImage(soundImg, "pack://application:,,,/View/UIMaterial/Image/volumeup.png");
            else
                SetImage(soundImg, "pack://application:,,,/View/UIMaterial/Image/volumeoff.png");
        }

        //Sound Button Mouse Click
        private void soundBtn_Click(object sender, EventArgs e)
        {
            if (soundOn)
                SetImage(soundImg, "pack://application:,,,/View/UIMaterial/Image/volumeoff_blue.png");
            else
                SetImage(soundImg, "pack://application:,,,/View/UIMaterial/Image/volumeup_blue.png");

            soundOn = !soundOn;
        }

        //Thred for updating the lyric
        public delegate void UpdateLyricCallback(string message);
        private void LyricThread()
        {
            for (int i = 0; i <= 1000000000; i++)
            {
                Thread.Sleep(1000);
                lyricText.Dispatcher.Invoke(
                    new UpdateLyricCallback(this.UpdateText),
                    new object[] { i.ToString() }
                );
            }
        }

        private void UpdateText(string message)
        {
            var musicTimeSpan = TimeSpan.FromMilliseconds(musicTime.Elapsed.TotalMilliseconds);
            string currentLyric = currentLyricFile.GetCurrentLyric(Convert.ToInt32(musicTimeSpan.TotalHours), Convert.ToInt32(musicTimeSpan.TotalMinutes),
                Convert.ToInt32(musicTimeSpan.TotalSeconds), Convert.ToInt32(musicTimeSpan.TotalMilliseconds - Convert.ToInt32(musicTimeSpan.TotalSeconds) * 1000));

            lyricText.Text = Convert.ToInt32(musicTimeSpan.TotalHours) + ":" + Convert.ToInt32(musicTimeSpan.TotalMinutes) + ":" + Convert.ToInt32(musicTimeSpan.TotalSeconds) + "  " + currentLyric;
        }

        private void UpdatePlayState(bool isPlay)
        {
            this._karaokeSystemModel.Playback.Playing = !this._karaokeSystemModel.Playback.Playing;

            if (this._karaokeSystemModel.Playback.Playing)
            {
                if (musicTime == null)
                    musicTime = System.Diagnostics.Stopwatch.StartNew();
                else
                    musicTime.Start();
                Thread test = new Thread(new ThreadStart(LyricThread));
                test.Start();

                var aviHeaderParser = new P2PKaraokeSystem.PlaybackLogic.AviHeaderParser();
                aviHeaderParser.LoadFile("Z:\\Code\\P2PKaraokeSystem\\VideoDatabase\\Video\\only_time.avi");

                AudioFrameReader frameReader = new AudioFrameReader();
                frameReader.Load(aviHeaderParser.AudioHeaderReader);
                frameReader.ReadFrameFully(aviHeaderParser.AudioHeaderReader);

                AudioPlayer audioPlayer = new AudioPlayer(this._karaokeSystemModel.Playback);

                audioPlayer.OpenDevice(aviHeaderParser.AudioHeaderReader.FormatInfo, delegate { });
                audioPlayer.WriteToStream(frameReader.FramePointer, frameReader.FrameSize);
            }
            else
            {
                musicTime.Stop();
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this._karaokeSystemModel.Playback.CurrentVideo = ((sender as ListView).SelectedItem as Model.VideoDatabase.Video);
            UpdatePlayState(true);
        }
    }
}
