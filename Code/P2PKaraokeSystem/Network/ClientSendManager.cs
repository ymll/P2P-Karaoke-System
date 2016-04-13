using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P2PKaraokeSystem.Network
{
    public class ClientSendManager : AbstractSendManager
    {
        private Int32 serverport = 12345;
        private String ServeripString = "127.0.0.1"; //"192.168.0.5";
        public ClientSendManager()
        {
            Form1 inputForm = new Form1();
          //  inputForm.Show();
            //msg box to collect server's ip address and port
        }
        public partial class Form1 : Form
        {
            public Form1()
            {
                InitializeComponent(); 
            }
            private void InitializeComponent()
            {
                this.AutoSize = true;
                this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                TextBox ipaddr = new TextBox();
                ipaddr.Text = "127.0.0.1";//"192.168.0.2";
                ipaddr.Location = new Point(15, 15);
                this.Controls.Add(ipaddr);
                TextBox portnumber = new TextBox();
                portnumber.Text = "12345";
                portnumber.Location = new Point(15, 40);
                this.Controls.Add(portnumber);
                Button btn = new Button();            
                btn.Text = "OK";
                btn.Location = new Point(25, 65);
                this.Controls.Add(btn);
                btn.Click += new EventHandler(btn_Click);
            }
            void btn_Click(object sender, EventArgs e)
            {
              

            }
        }
     
        public ClientSendManager(String Serverip, Int32 setport)
        {
            this.serverport = setport;
            this.ServeripString = Serverip;
        }
        // TODO
        public override int SendTCP(byte[] sendBuffer, int from, int size)
        {  
            try
            {          
                TcpClient client = new TcpClient();
                Console.Write(ServeripString);
                Console.Write(serverport);
                client.Connect(ServeripString, serverport);
                NetworkStream networkStream = client.GetStream();
                networkStream.Write(sendBuffer, from, size);
                networkStream.Flush();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
            return 0;
        }
        const string broadcastAddress = "255.255.255.255";
        UdpClient sendingClient;
        private void InitializeSender(byte[] data)
        {
            sendingClient = new UdpClient(broadcastAddress, 12345);
            sendingClient.EnableBroadcast = true;
            sendingClient.Send(data, data.Length);
        }
        // TODO
        public override int SendUDP(byte[] sendBuffer, int from, int size)
        {

            return 0;
        }
    }
}
