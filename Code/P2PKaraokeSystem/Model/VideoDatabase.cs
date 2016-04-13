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
using System.ComponentModel;
using P2PKaraokeSystem.PlaybackLogic.Decode;


namespace P2PKaraokeSystem.Model
{
    public class VideoDatabase : AbstractNotifyPropertyChanged
    {
        public ObservableCollection<Video> Videos { get; private set; }
        public ObservableCollection<Video> allVideos { get; private set; }
        public ObservableCollection<Video> VideosFromPeer { get; set; }
        public static List<ServerStruct> clientList = new List<ServerStruct>();

        private string DatabaseFileLocation;
        private readonly string[] DIRECTORIES_FOR_DB_FILE = new string[]{
                Environment.CurrentDirectory,
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Path.GetTempPath()
            };
        private const string DATABASE_FILENAME = "KaraokeSystemDatabase.csv";

        public VideoDatabase()
        {
            Videos = new ObservableCollection<Video>();
            allVideos = new ObservableCollection<Video>();

            LoadFromFile();
        }

        public void LoadFromFile()
        {
            foreach (string directory in DIRECTORIES_FOR_DB_FILE)
            {
                string filepath = Path.Combine(directory, DATABASE_FILENAME);
                try
                {
                    Directory.CreateDirectory(directory);
                    LoadFromFile(filepath);
                    DatabaseFileLocation = filepath;
                    break;
                }
                catch (Exception) { }
            }

            SaveToFile();
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
            //send to peer for query
            clientList.ForEach(delegate(ServerStruct serverstruce)
            {
                ClientSendManager manager = new ClientSendManager(serverstruce.serveripString, serverstruce.serverport);
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
                    long lengthInMillisecond;
                    string videoFilePath = csv.GetField<string>("VideoFilePath");

                    if (IsVideo(videoFilePath, out lengthInMillisecond))
                    {
                        Performer performer = new Performer(csv.GetField<string>("PerformerName"));
                        Lyric lyric = new Lyric(csv.GetField<string>("LyricFilePath"));
                        Video video = new Video(csv.GetField<string>("VideoTitle"), csv.GetField<string>("VideoFilePath"), csv.GetField<long>("LengthInMillisecond"), performer, lyric);

                        Videos.Add(video);
                        allVideos.Add(video);
                    }
                }
            }
        }

        public bool IsVideo(string filepath, out long lengthInMillisecond)
        {
            MediaDecodeInfo mediaDecodeInfo = new MediaDecodeInfo();
            MediaLoader loader = new MediaLoader(mediaDecodeInfo, null);
            try
            {
                loader.RetrieveFormatAndStreamInfo(filepath);
            }
            catch (Exception)
            {
                lengthInMillisecond = -1;
                return false;
            }

            lengthInMillisecond = mediaDecodeInfo.LengthInMillisecond;
            return true;
        }

        private void LoadSearch(string keywords)
        {

            Video[] tempVideoArr = new Video[allVideos.Count];
            allVideos.CopyTo(tempVideoArr, 0);
            int videoCount = allVideos.Count;

            Videos.Clear();
            string[] words = keywords.Split(' ');

            for (int k = 0; k < words.Count(); k++)
            {
                for (int i = 0; i < videoCount; i++)
                {
                    if (tempVideoArr[i].Performer.Name == words[k] || tempVideoArr[i].Title == words[k])
                    {
                        Videos.Add(tempVideoArr[i]);
                    }
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

        public ObservableCollection<Video> LoadSearchToPeer(string keywords)
        {
            String[] words = keywords.Split(' ');
            ObservableCollection<Video> VideosPeer = new ObservableCollection<Video>();

            Video[] tempVideoArr = new Video[allVideos.Count];
            allVideos.CopyTo(tempVideoArr, 0);
            int videoCount = allVideos.Count;
            bool contain = false;

            //var index = allVideos.Where(vid => (words.Any(word => vid.Title.Contains(word)) || words.Any(word => vid.Performer.Name.Contains(word))));
            for (int i = 0; i < videoCount; i++)
            {
                contain = false;
                for (int k = 0; i < words.Count(); k++)
                {
                    //if (tempVideoArr[i].Performer.Name.Contains(words[k])) contain = true;
                    //if (tempVideoArr[i].Title.Contains(words[k])) contain = true;
                    if (tempVideoArr[i].Performer.Name == words[k] || tempVideoArr[i].Title == words[k]) contain = true;
                }
                if (contain) VideosPeer.Add(tempVideoArr[i]);
            }
            if (VideosPeer.Count == 0) return null;
            else return VideosPeer;

        }

        public void SaveIpPort(string ipAddress, string port)
        {
            Int32 int32_port = Int32.Parse(port);
            ServerStruct serverstruct = new ServerStruct(ipAddress, int32_port);
            if (!clientList.Contains(serverstruct)) clientList.Add(serverstruct);
        }

        public string SaveToFile()
        {
            if (DatabaseFileLocation != null)
            {
                if (SaveToFile(DatabaseFileLocation))
                {
                    return DatabaseFileLocation;
                }
            }

            foreach (string directory in DIRECTORIES_FOR_DB_FILE)
            {
                string filepath = Path.Combine(directory, DATABASE_FILENAME);

                if (SaveToFile(filepath))
                {
                    DatabaseFileLocation = filepath;
                    return filepath;
                }
            }

            return null;
        }

        private bool SaveToFile(string path)
        {
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(path))
                {
                    Save(streamWriter);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private String SaveToText()
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
                csv.WriteField<string>("LengthInMillisecond");
                csv.WriteField<string>("LyricFilePath");
                csv.WriteField<string>("VideoTitle");
                csv.WriteField<string>("VideoFilePath");
                csv.NextRecord();

                foreach (Video video in Videos)
                {
                    csv.WriteField<string>(video.Performer.Name);
                    csv.WriteField<long>(video.LengthInMillisecond);
                    csv.WriteField<string>(video.Lyric.FilePath);
                    csv.WriteField<string>(video.Title);
                    csv.WriteField<string>(video.FilePath);
                    csv.NextRecord();
                }
            }
        }

        public class Video
        {
            public String Title { get; set; }
            public String FilePath { get; set; }
            public long LengthInMillisecond { get; set; }
            public Performer Performer { get; set; }
            public Lyric Lyric { get; set; }

            public Video(String filePath, long lengthInMillisecond)
                : this(Path.GetFileNameWithoutExtension(filePath), filePath, lengthInMillisecond, new Performer(""), new Lyric(""))
            {

            }

            public Video(String title, String filePath, long lengthInMillisecond, Performer performer, Lyric lyric)
            {
                this.Title = title;
                this.FilePath = filePath;
                this.LengthInMillisecond = lengthInMillisecond;
                this.Performer = performer;
                this.Lyric = lyric;
            }
        }

        public class Performer
        {
            public String Name { get; set; }

            public Performer(String name)
            {
                this.Name = name;
            }
        }

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
