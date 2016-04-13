using Microsoft.Win32;
using P2PKaraokeSystem.Model;
using P2PKaraokeSystem.PlaybackLogic;
using P2PKaraokeSystem.PlaybackLogic.Decode;
using P2PKaraokeSystem.PlaybackLogic.Native.FFmpeg;
using System;
using System.Collections.Generic;
using System.IO;
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
        int preVol = 127;
        public string searchKeyWords;

        public MainUI()
        {
            //testing code:
            /*
            ClientReceiveManager clientRecv = new ClientReceiveManager("127.0.0.1",12345,1024*32);
            clientRecv.RegisterListener(PacketType.PLAY_REQUEST, new PlayRequestListener());
            clientRecv.RegisterListener(PacketType.MEDIA_INFO, new MediaInfoReceiveListener());
            clientRecv.RegisterListener(PacketType.VIDEO_STREAM, new VideoStreamReceiveListener());
            clientRecv.RegisterListener(PacketType.SUBTITLE, new SubtitleReceiveListener());
            clientRecv.RegisterListener(PacketType.LYRIC_REQUEST, new LyricRequestListener());
            clientRecv.StartReceiveTcpPacket();


            string sendingString = "../../VideoDatabase/Video/only_time.avi";
            Byte[] data = Encoding.ASCII.GetBytes(sendingString);
            Byte[] sendBytes;

            */
            /* 
            ClientSendManager cl =  new ClientSendManager();
            cl.AddPayload(out sendBytes, data, PacketType.PLAY_REQUEST);
            cl.SendTCP(sendBytes,0,sendBytes.Length);      
            sendingString = "../../VideoDatabase/Lyrics/Enya - Only Time.lrc";
            data = Encoding.ASCII.GetBytes(sendingString);         
            c1.AddPayload(out sendBytes, data, PacketType.LYRIC_REQUEST);
            c1.SendTCP(sendBytes, 0, sendBytes.Length);
            */
            /* 
            ServerSendManager c2 = new ServerSendManager();
            c2.AddPayload(out sendBytes, data, PacketType.PLAY_REQUEST);
            c2.SendTCP(sendBytes, 0, sendBytes.Length);
            */
            //end testing code

            FFmpegLoader.LoadFFmpeg();
            InitializeComponent();

            this._karaokeSystemModel = (KaraokeSystemModel)this.DataContext;
            new LyricPlayer(this._karaokeSystemModel.Playback, this._karaokeSystemModel.View);
            new FFmpegDecoder(this._karaokeSystemModel.View, this._karaokeSystemModel.Playback).StartAsync();
        }

        private void playImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (this._karaokeSystemModel.Playback.State)
            {
                case PlayState.NotPlaying:
                    this._karaokeSystemModel.Playback.State = PlayState.Playing;
                    break;
                case PlayState.Playing:
                    this._karaokeSystemModel.Playback.State = PlayState.NotPlaying;
                    break;
                default:
                    break;
            }
        }

        private void soundImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this._karaokeSystemModel.Playback.Volume == 0)
            {
                if (preVol != 0)
                {
                    this._karaokeSystemModel.Playback.Volume = preVol;
                }
                else
                {
                    preVol = 255;
                    this._karaokeSystemModel.Playback.Volume = preVol;
                }
            }
            else
            {
                this._karaokeSystemModel.Playback.Volume = 0;
            }
        }

        private void soundMinImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this._karaokeSystemModel.Playback.Volume != 0)
            {
                if (preVol >= 25)
                {
                    preVol -= 25;
                }
                else
                {
                    preVol = 0;
                }

                this._karaokeSystemModel.Playback.Volume = preVol;
            }
        }

        private void soundPlusImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this._karaokeSystemModel.Playback.Volume != 255)
            {
                preVol += 25;
                this._karaokeSystemModel.Playback.Volume = preVol;
            }
        }


        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this._karaokeSystemModel.Playback.CurrentVideo = ((sender as ListView).SelectedItem as Model.VideoDatabase.Video);
            this._karaokeSystemModel.Playback.State = PlayState.Playing;
            this._karaokeSystemModel.Playback.Volume = 270;
            preVol = 270;
        }

        private void screenImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this._karaokeSystemModel.View.VideoScreenBitmap != null)
            {
                this._karaokeSystemModel.View.VideoScreenBitmap.Dispatcher.Invoke(() =>
                {
                    string filepath = "screenshots/" + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".jpg";
                    Directory.CreateDirectory(filepath);
                    using (FileStream stream = new FileStream(filepath, FileMode.Create))
                    {
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();

                        encoder.Frames.Add(BitmapFrame.Create(this._karaokeSystemModel.View.VideoScreenBitmap));
                        encoder.Save(stream);
                    }
                    MessageBox.Show("Your screenshot is saved at \"" + filepath + "\"", "Screen captured!");
                });
            }
        }

        private void connectionImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            popUp.IsOpen = true;
            this._karaokeSystemModel.VideoDatabase.SaveIpPort(ipAdd.Text, portNum.Text);
        }

        private void PopUp_OK_Click(object sender, RoutedEventArgs e)
        {
            popUp.IsOpen = false;
        }

        private void RemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = Playlist.SelectedIndex;
            if (selectedIndex >= 0)
            {
                VideoDatabase.Video video = this._karaokeSystemModel.VideoDatabase.Videos[selectedIndex];
                this._karaokeSystemModel.VideoDatabase.Videos.Remove(video);
                this._karaokeSystemModel.VideoDatabase.SaveToFile();
            }
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            popUpEdit.IsOpen = true;
            int selectedIndex = Playlist.SelectedIndex;
            VideoDatabase.Video tempVideo = Playlist.SelectedItem as VideoDatabase.Video;
            saveVideoIndex(selectedIndex);
            EditTitle.Text = tempVideo.Title;
            EditSinger.Text = tempVideo.Performer.Name;
        }

        int videoIndex;
        private void saveVideoIndex(int selectedIndex)
        {
            videoIndex = selectedIndex;
        }

        private void PopUpEdit_OK_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = videoIndex;
            VideoDatabase.Video selectedVideo = this._karaokeSystemModel.VideoDatabase.Videos[selectedIndex];
            selectedVideo.Title = EditTitle.Text;
            selectedVideo.Performer.Name = EditSinger.Text;
            Playlist.Items.Refresh();
            popUpEdit.IsOpen = false;
            this._karaokeSystemModel.VideoDatabase.SaveToFile();
        }

        private void searchEnterDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                searchKeyWords = searchBox.Text;
            }
        }

        private void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ReadOnlyChecked = true;
            fileDialog.Multiselect = true;

            bool? isOpened = fileDialog.ShowDialog();

            if (isOpened.GetValueOrDefault(false))
            {
                foreach (string fileName in fileDialog.FileNames)
                {
                    long lengthInMillisecond;

                    if (this._karaokeSystemModel.VideoDatabase.IsVideo(fileName, out lengthInMillisecond))
                    {
                        VideoDatabase.Video video = new VideoDatabase.Video(fileName, lengthInMillisecond);

                        if (!this._karaokeSystemModel.VideoDatabase.Videos.Any(v => fileName.Equals(v.FilePath)))
                        {
                            this._karaokeSystemModel.VideoDatabase.Videos.Add(video);
                        }
                    }
                }
                this._karaokeSystemModel.VideoDatabase.SaveToFile();
            }
        }
    }
}
