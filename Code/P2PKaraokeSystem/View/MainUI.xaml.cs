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
        int preVol = 127;


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

            InitializeComponent();
            FFmpegLoader.LoadFFmpeg();

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
            popUp.IsOpen = true;
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
                Playlist.Items.Refresh();
            }
        }
    }
}
