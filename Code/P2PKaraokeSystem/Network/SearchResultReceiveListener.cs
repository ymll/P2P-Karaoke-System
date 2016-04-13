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
    public class SearchResultReceiveListener : DataReceiveListener
    {
        public void OnDataReceived(PacketType packetType, byte[] data, String ipAddress, Int32 portNo)  //TODO: modify all
        {
            Console.WriteLine(packetType);
            Console.WriteLine("SearchResultReceiveListener OnDataReceived\n");
            //data is collection of video, parse them out
            //call method in VideoDatabase: TODO (add collection together + save the PIs for that video)
            //finished!! :DDD

            //Source: http://stackoverflow.com/questions/1446547/how-to-convert-an-object-to-a-byte-array-in-c-sharp
            try
            {
                using (var memStream = new MemoryStream())
                {
                    var binForm = new BinaryFormatter();
                    memStream.Write(data, 0, data.Length);
                    memStream.Seek(0, SeekOrigin.Begin);
                    var obj = binForm.Deserialize(memStream);
                    ObservableCollection<Model.VideoDatabase.SendableVideo> videoCollection = (ObservableCollection<Model.VideoDatabase.SendableVideo>) obj;
                    Model.VideoDatabase.LoadResultFromPeer(videoCollection, ipAddress, portNo);    //TODO: LoadResultFromPeer
                }
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }

        }
    }
}