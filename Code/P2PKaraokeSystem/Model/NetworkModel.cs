using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Model
{
    public class NetworkModel
    {
        public bool IsNetworkModeOn { get; set; }
        public int LocalPort { get; set; }
        public List<TcpClient> Peers { get; private set; }

        public NetworkModel()
        {
            this.IsNetworkModeOn = true;
            this.LocalPort = -1;
            this.Peers = new List<TcpClient>();
        }
    }
}
