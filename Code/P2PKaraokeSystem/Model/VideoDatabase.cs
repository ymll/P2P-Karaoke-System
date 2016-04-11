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

namespace P2PKaraokeSystem.Model
{
    public class VideoDatabase : AbstractNotifyPropertyChanged
    {
        public ObservableCollection<Video> Videos { get; private set; }
        public ObservableCollection<Video> allVideos { get; private set; }

        public VideoDatabase()
        {
            Videos = new ObservableCollection<Video>();
            allVideos = new ObservableCollection<Video>();

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
                    IOneLineLyric oneLineLyric = _lyricFile.AfterOrAt(timeSpan);
                    return oneLineLyric.Content;
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }
    }
}
