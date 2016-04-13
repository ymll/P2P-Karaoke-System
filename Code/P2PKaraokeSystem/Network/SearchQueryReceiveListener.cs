using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace P2PKaraokeSystem.Network
{
    public class SearchQueryReceiveListener : DataReceiveListener
    {
        public void OnDataReceived(PacketType packetType, byte[] data, String ipAddress, Int32 portNo)  //TODO: change all related function
        {
            Console.WriteLine(packetType);
            Console.WriteLine("SearchQueryReceiveListener OnDataReceived\n");

            //byte[] to keyworks
            //call search for peer function
            //send the return here (parse the object)
            String keyworks = ASCIIEncoding.ASCII.GetString(data);
            ObservableCollection<Model.VideoDatabase.SendableVideo> videoCollection = Model.VideoDatabase.LoadSearchToPeer(keyworks);
            if (videoCollection == null) return;
            else
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    using (var ms = new MemoryStream())
                    {
                        bf.Serialize(ms, videoCollection);
                        byte[] collectionByte = ms.ToArray();
                        ClientSendManager manager = new ClientSendManager(ipAddress, portNo);
                        byte[] sendbuff;
                        manager.AddPayload(out sendbuff, collectionByte, PacketType.SEARCH_RESULT);
                        manager.SendTCP(sendbuff, 0, sendbuff.Length);
                    }
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                    throw;
                }

            }

        }
    }
}