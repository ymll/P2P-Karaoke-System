﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.Network
{
    public class SearchResultReceiveListener : DataReceiveListener
    {
        public void OnDataReceived(PacketType packetType, byte[] data)
        {
            Console.WriteLine(packetType);
            Console.WriteLine("SearchResultReceiveListener OnDataReceived\n");
        }
    }
}