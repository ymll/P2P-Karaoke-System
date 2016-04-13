using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Model
{
    public class NetworkModel : AbstractNotifyPropertyChanged
    {
        public bool IsNetworkModeOn { get; set; }
        public List<TcpClient> Peers { get; private set; }

        private int _localPort;
        public int LocalPort
        {
            get { return _localPort; }
            set { SetField(ref _localPort, value, "LocalPort"); }
        }

        public NetworkModel()
        {
            this.IsNetworkModeOn = true;
            this.LocalPort = -1;
            this.Peers = new List<TcpClient>();
        }
    }
}
