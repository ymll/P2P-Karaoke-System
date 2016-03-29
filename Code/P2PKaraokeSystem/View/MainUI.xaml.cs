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
