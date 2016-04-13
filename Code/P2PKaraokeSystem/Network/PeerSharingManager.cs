using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public class PeerSharingManager
    {
        private readonly NetworkModel networkModel;
        private TcpListener tcpListener;

        public PeerSharingManager(NetworkModel networkModel)
        {
            this.networkModel = networkModel;
        }

        public void StartSharing()
        {
            StartLocalServer();
        }

        public void StopSharing()
        {
            this.tcpListener.Stop();
        }

        private void StartLocalServer()
        {
            this.tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
            this.tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.tcpListener.Start();
            this.networkModel.LocalPort = (this.tcpListener.LocalEndpoint as IPEndPoint).Port;
        }
    }
}
