using CsvHelper;
using Kfstorm.LrcParser;
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

        public VideoDatabase()
        {
            Videos = new ObservableCollection<Video>();

            try
            {
                LoadFromFile("Z:\\Users\\sonia\\note\\year4\\sem2\\CSCI3280\\Project\\Code\\P2PKaraokeSystem\\VideoDatabase\\db.csv");
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
