using P2PKaraokeSystem.PlaybackLogic;
using P2PKaraokeSystem.PlaybackLogic.Native.FFmpeg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        bool isStopping = true;
        bool soundOn = true;

        public MainUI()
        {
            //testing code:
            /*
            new ClientReceiveManager().StartReceiveTcpPacket(); 
            */
            /*
            string sendingString = "Testing string.......";
            Byte[] data = Encoding.ASCII.GetBytes(sendingString);
            Byte[] sendBytes;
            */
            /* 
            ClientSendManager cl =  new ClientSendManager();
            cl.AddPayload(out sendBytes, data, PacketType.AUDIO_STREAM);
            cl.SendTCP(sendBytes,0,sendBytes.Length);      
            */
            /* 
            ServerSendManager c2 = new ServerSendManager();
            c2.AddPayload(out sendBytes, data, PacketType.SUBTITLE);
            c2.SendTCP(sendBytes, 0, sendBytes.Length);
            */
            //end testing code
        
            InitializeComponent();
            FFmpegLoader.LoadFFmpeg();
        }

        //Set Image Source for an image throught Uri
        private void SetImage(Image imageName, string path)
        {
            imageName.BeginInit();
            imageName.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            imageName.EndInit();

        }

        //Play and Stop Button Enter
        private void playBtn_MouseEnter(object sender, EventArgs e)
        {
            if (isStopping)
                SetImage(playImg, "pack://application:,,,/View/UIMaterial/Image/play_blue.png");
            else
                SetImage(playImg, "pack://application:,,,/View/UIMaterial/Image/stop_blue.png");
        }

        //Play and Stop Mouse Leave
        private void playBtn_MouseLeave(object sender, EventArgs e)
        {
            if (isStopping)
                SetImage(playImg, "pack://application:,,,/View/UIMaterial/Image/play.png");
            else
                SetImage(playImg, "pack://application:,,,/View/UIMaterial/Image/stop.png");
        }

        //Play and Stop Mouse Click
        private void playBtn_Click(object sender, EventArgs e)
        {
            if (isStopping)
                SetImage(playImg, "pack://application:,,,/View/UIMaterial/Image/stop_blue.png");
            else
                SetImage(playImg, "pack://application:,,,/View/UIMaterial/Image/play_blue.png");

            isStopping = !isStopping;

            var aviHeaderParser = new P2PKaraokeSystem.PlaybackLogic.AviHeaderParser();
            aviHeaderParser.LoadFile("Z:\\Code\\P2PKaraokeSystem\\VideoDatabase\\Video\\only_time.avi");

            AudioFrameReader frameReader = new AudioFrameReader();
            frameReader.Load(aviHeaderParser.AudioHeaderReader);
            frameReader.ReadFrameFully(aviHeaderParser.AudioHeaderReader);

            AudioPlayer audioPlayer = new AudioPlayer();

            audioPlayer.OpenDevice(aviHeaderParser.AudioHeaderReader.FormatInfo, delegate { });
            audioPlayer.WriteToStream(frameReader.FramePointer, frameReader.FrameSize);
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
    }
}
