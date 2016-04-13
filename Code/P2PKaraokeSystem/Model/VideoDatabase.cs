using CsvHelper;
using Kfstorm.LrcParser;
using P2PKaraokeSystem.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace P2PKaraokeSystem.Model
{
    public class VideoDatabase : AbstractNotifyPropertyChanged
    {
        public static ObservableCollection<Video> Videos { get; private set; }
        public static ObservableCollection<Video> allVideos { get; private set; }
        //public ObservableCollection<Video> VideosFromPeer { get; set; }
        public static Dictionary<Video, List<ServerStruct>> VideosFromPeer { get; private set; }
        public static List<ServerStruct> clientList { get; private set; }

        public VideoDatabase()
        {
            Videos = new ObservableCollection<Video>();
            allVideos = new ObservableCollection<Video>();
            VideosFromPeer = new Dictionary<Video, List<ServerStruct>>();
            clientList = new List<ServerStruct>();

            try
            {
                LoadFromFile("..\\..\\VideoDatabase\\db.csv");
            }
            catch (Exception)
            {

            }
        }

        public void LoadFromFile(string path)
        {
            Load(new StreamReader(path));
        }

        public void LoadFromText(string text)
        {
            Load(new StringReader(text));
        }

        public void LoadForSearch(string path, string keywords)
        {
            LoadSearch(keywords);
            //send to peer for query : TODO add button to send query, otherwise too much lol
            //clear VideosFromPeer

            clientList.ForEach(delegate (ServerStruct serverstruct)
            {
                System.Diagnostics.Debug.WriteLine("Called ForEach\n");
                ClientSendManager manager = new ClientSendManager(serverstruct.serveripString, serverstruct.serverport);
                byte[] sendKeywords = Encoding.ASCII.GetBytes(keywords);
                byte[] sendbuff;
                manager.AddPayload(out sendbuff, sendKeywords, PacketType.SEARCH_QUERY);
                manager.SendTCP(sendbuff, 0, sendbuff.Length);
            });
        }

        private void Load(TextReader textReader)
        {
            using (CsvReader csv = new CsvReader(textReader))
            {
                while (csv.Read())
                {
                    Performer performer = new Performer(csv.GetField<string>("PerformerName"));
                    Lyric lyric = new Lyric(csv.GetField<string>("LyricFilePath"));
                    Video video = new Video(csv.GetField<string>("VideoTitle"), csv.GetField<string>("VideoFilePath"), performer, lyric);

                    Videos.Add(video);
                    allVideos.Add(video);
                }
            }
           //foreach(Video video in Videos)
           // {
           //     System.Diagnostics.Debug.WriteLine(video.Title)
           // }

            ObservableCollection<SendableVideo> collectionCheck = new ObservableCollection<SendableVideo>();

            foreach(Video video in Videos)
            {
                SendableVideo sendvideo = new SendableVideo(video.Title, video.FilePath, video.Performer);
                collectionCheck.Add(sendvideo);
            }

            //BinaryFormatter bf = new BinaryFormatter();
            //byte[] collectionByte;
            //using (var ms = new MemoryStream())
            //{
            //    System.Diagnostics.Debug.WriteLine("\n stage1");
            //    try
            //    {
            //        bf.Serialize(ms, collectionCheck);
            //    } catch (SerializationException e) { System.Diagnostics.Debug.WriteLine(e.Message); }
               
            //    System.Diagnostics.Debug.WriteLine("\n stage2");
            //    collectionByte = ms.ToArray();
            //}
            //using (var memStream = new MemoryStream())
            //{
            //    System.Diagnostics.Debug.WriteLine("\n stage3");
            //    var binForm = new BinaryFormatter();
            //    memStream.Write(collectionByte, 0, collectionByte.Length);
            //    System.Diagnostics.Debug.WriteLine("\n stage4");
            //    memStream.Seek(0, SeekOrigin.Begin);
            //    System.Diagnostics.Debug.WriteLine("\n stage5");
            //    var obj = binForm.Deserialize(memStream);
            //    System.Diagnostics.Debug.WriteLine("\n stage6");
            //    ObservableCollection<Model.VideoDatabase.SendableVideo> videoCollection = (ObservableCollection<Model.VideoDatabase.SendableVideo>)obj;
            //    System.Diagnostics.Debug.WriteLine("\n\n" + videoCollection[0].Title + "\n\n");
            //}
        }

        private void LoadSearch(string keywords)
        {
            Console.WriteLine(keywords);
            keywords = keywords.ToLower();
            Video[] tempVideoArr = new Video[allVideos.Count];
            allVideos.CopyTo(tempVideoArr, 0);
            int videoCount = allVideos.Count;

            Videos.Clear();
            string[] words = keywords.Split(' ');
            string temp;
            bool contain = false;
            Console.WriteLine(videoCount + " " + words.Count());
            for (int i = 0; i < videoCount; i++)
            {
                contain = false;
                for (int k = 0; k < words.Count(); k++)
                {
                    Console.WriteLine(tempVideoArr[i].Performer.Name);
                    temp = tempVideoArr[i].Performer.Name.ToLower();
                    if (temp.Contains(words[k])) contain = true;
                    temp = tempVideoArr[i].Title.ToLower();
                    if (temp.Contains(words[k])) contain = true;
                    
                }
                if (contain)
                {
                    Console.WriteLine("contain!");
                    Videos.Add(tempVideoArr[i]);
                }
            }

            if (keywords == "")
            {
                Videos.Clear();
                for (int i = 0; i < videoCount; i++)
                {
                    Videos.Add(allVideos[i]);
                }
            }
        }

        public static void LoadResultFromPeer(ObservableCollection<SendableVideo> videoCollection, String ipAddress, Int32 portNo)
        {
            //for each entry, if is new, add
            //else 
            //Console.WriteLine(videoCollection.Count());
            //Console.WriteLine(videoCollection[0].Title + " " + videoCollection[0].FilePath);
            foreach (SendableVideo sendablevideo in videoCollection)
            {
                Video video = new Video(sendablevideo.Title, sendablevideo.FilePath, sendablevideo.Performer, null);
                //Console.WriteLine("video created");
                //Console.WriteLine(video.Title);
                List<ServerStruct> tempList;
                ServerStruct serverstruct = new ServerStruct(ipAddress, portNo);
                if (VideosFromPeer.TryGetValue(video, out tempList))
                {
                    if (!VideosFromPeer[video].Contains(serverstruct)) {
                        VideosFromPeer[video].Add(serverstruct);
                    }
                }
                else
                {
                    VideosFromPeer.Add(video, new List<ServerStruct>());
                    VideosFromPeer[video].Add(serverstruct);
                }
                if (!Videos.Contains(video))
                {
                    //todo: check if any video has same name
                    var exist = Videos.Where(x => x.Title==video.Title).FirstOrDefault();
                    if (exist == null)
                    {
                        Console.WriteLine("going to add to Videos list");
                        //System.Windows.Threading.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action(()=>View));     //TODO: error in refreshing Videos list
                        Videos.Add(video);
                        Console.WriteLine(Videos.Count());
                    }
                }
            }
        }

        public static ObservableCollection<SendableVideo> LoadSearchToPeer(string keywords)
        {
            Console.WriteLine("Called LoadSearchToPeer");
            keywords = keywords.ToLower();
            String[] words = keywords.Split(' ');
            Console.WriteLine("number of words: "+words.Count());
            ObservableCollection<SendableVideo> VideosPeer = new ObservableCollection<SendableVideo>();
            String temp;
            
            Video[] tempVideoArr = new Video[allVideos.Count];
            allVideos.CopyTo(tempVideoArr, 0);
            int videoCount = allVideos.Count;
            bool contain = false;
            //Console.WriteLine("Inited");
            //Console.WriteLine(videoCount + " " + words.Count());
            //var index = allVideos.Where(vid => (words.Any(word => vid.Title.Contains(word)) || words.Any(word => vid.Performer.Name.Contains(word))));
            for (int i = 0; i < videoCount; i++)
            {
                contain = false;
                //Console.WriteLine("videoCount = " + i);
                for (int k = 0; k< words.Count(); k++)
                {
                    //Console.WriteLine("words.Count() = " + k);
                    temp = tempVideoArr[i].Performer.Name.ToLower();
                    if (temp.Contains(words[k])) contain = true;
                    temp = tempVideoArr[i].Title.ToLower();
                    if (temp.Contains(words[k])) contain = true;
                    //if (tempVideoArr[i].Performer.Name == words[k] || tempVideoArr[i].Title == words[k]) contain = true;
                }
                if (contain)
                {
                    //Console.WriteLine("Contain!");
                    SendableVideo sendvideo = new SendableVideo(tempVideoArr[i].Title, tempVideoArr[i].FilePath, tempVideoArr[i].Performer);
                    //Console.WriteLine("before Add");
                    VideosPeer.Add(sendvideo);
                }
            }
            //Console.WriteLine(VideosPeer.Count());
            if (VideosPeer.Count == 0) return null;
            else
            {
                //Console.WriteLine(VideosPeer[0].Title + " " + VideosPeer[0].FilePath);
                return VideosPeer;
            }
            
        }

        public void SaveIpPort(string ipAddress, string port)
        {
            Int32 int32_port = Int32.Parse(port);
            ServerStruct serverstruct = new ServerStruct(ipAddress, int32_port);
            if (!clientList.Contains(serverstruct)) clientList.Add(serverstruct);
        }

        public void SendVideoRequest(Video selectedVideo)
        {
            try
            {
                List<ServerStruct> serverlist = VideosFromPeer[selectedVideo];
                serverlist.ForEach(delegate (ServerStruct serverstruct)
                {
                    ClientSendManager manager = new ClientSendManager(serverstruct.serveripString, serverstruct.serverport);
                    byte[] filepath = Encoding.ASCII.GetBytes(selectedVideo.FilePath);
                    byte[] sendbuff;
                    manager.AddPayload(out sendbuff, filepath, PacketType.PLAY_REQUEST);
                    manager.SendTCP(sendbuff, 0, sendbuff.Length);
                });
            }
            catch
            {
                Console.WriteLine("error in SendVideoRequest");
            }
        }

        public void SaveToFile(string path)
        {
            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                Save(streamWriter);
            }
        }

        public String SaveToText()
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                Save(stringWriter);
                return stringWriter.ToString();
            }
        }

        private void Save(TextWriter textWriter)
        {
            using (CsvWriter csv = new CsvWriter(textWriter))
            {
                csv.WriteField<string>("PerformerName");
                csv.WriteField<string>("LyricFilePath");
                csv.WriteField<string>("VideoTitle");
                csv.WriteField<string>("VideoFilePath");
                csv.NextRecord();

                foreach (Video video in Videos)
                {
                    csv.WriteField<string>(video.Performer.Name);
                    csv.WriteField<string>(video.Lyric.FilePath);
                    csv.WriteField<string>(video.Title);
                    csv.WriteField<string>(video.FilePath);
                    csv.NextRecord();
                }
            }
        }

        [Serializable]
        public class Video
        {
            public String Title { get; set; }
            public String FilePath { get; set; }
            public Performer Performer { get; set; }
            public Lyric Lyric { get; set; }

            public Video(String title, String filePath, Performer performer, Lyric lyric)
            {
                this.Title = title;
                this.FilePath = filePath;
                this.Performer = performer;
                this.Lyric = lyric;
            }
        }

        [Serializable]
        public class SendableVideo
        {
            public String Title { get; set; }
            public String FilePath { get; set; }
            public Performer Performer { get; set; }
            public SendableVideo(string title, String filePath, Performer performer)
            {
                this.Title = title;
                this.FilePath = filePath;
                this.Performer = performer;
            }
        }

        [Serializable]
        public class Performer
        {
            public String Name { get; set; }

            public Performer(String name)
            {
                this.Name = name;
            }
        }

        [Serializable]
        public class Lyric
        {
            public String FilePath { get; private set; }
            private ILrcFile _lyricFile;

            public Lyric(String filePath)
            {
                if (!File.Exists(filePath))
                {
                    Byte[] data = Encoding.ASCII.GetBytes(filePath);
                    Byte[] sendBytes;
                    ServerSendManager cl = new ServerSendManager();
                    cl.AddPayload(out sendBytes, data, PacketType.LYRIC_REQUEST);
                    cl.SendTCP(sendBytes, 0, sendBytes.Length);
                }
                Load(filePath);
            }

            public void Load(String filePath)
            {
                try
                {
                    string text = File.ReadAllText(filePath);
                    _lyricFile = LrcFile.FromText(text);
                    this.FilePath = filePath;
                }
                catch (Exception)
                {
                    this.FilePath = "";
                }
            }

            public String GetCurrentLyric(int currentMillisecond)
            {
                try
                {
                    TimeSpan timeSpan = TimeSpan.FromMilliseconds(currentMillisecond);
                    if (_lyricFile != null)
                    {
                        IOneLineLyric oneLineLyric = _lyricFile.AfterOrAt(timeSpan);
                        return oneLineLyric.Content;
                    }
                }
                catch (Exception) { }
                return "";
            }
        }
    }
}
