using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public class ServerStruct   //for searching to inputed ip
    {
        public String serveripString = "127.0.0.1";//"192.168.0.5";
        public Int32 serverport = 12345;
        public ServerStruct(String serverip, Int32 setport)
        {
            this.serveripString = serverip;
            this.serverport = setport;
        }
    }
}
