using System;
using System.Collections.Generic;
using System.IO;

namespace WPF2
{
    public class SubRip : ISubtitlesPlugin
    {
        public string Name { get => "SubRip"; }
        public string Extension { get => ".srt"; }

        public ICollection<Subtitles> Load(string path)
        {
            List<Subtitles> subtitles = new List<Subtitles>();

            string[] lines = File.ReadAllLines(path);

            for (int i = 0; i < lines.Length; i++)
            {
                if (int.TryParse(lines[i], out _))
                {
                    i++;
                    string timeLine = lines[i];
                    string[] timeParts = timeLine.Split("-->");
                    string beginTimeString = timeParts[0].Trim();
                    string endTimeString = timeParts[1].Trim();

                    i++;
                    string text = lines[i];

                    TimeSpan beginTime = TimeSpan.Parse(beginTimeString);
                    TimeSpan endTime = TimeSpan.Parse(endTimeString);

                    Subtitles subtitle = new Subtitles(beginTime, endTime, text, "");

                    subtitles.Add(subtitle);
                }
            }

            return subtitles;
        }
        public void Save(string path, ICollection<Subtitles> subtitles)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                int index = 1;
                foreach (Subtitles subtitle in subtitles)
                {
                    writer.WriteLine(index);
                    writer.WriteLine($"{FormatTime(subtitle.ShowTime)} --> {FormatTime(subtitle.HideTime)}");
                    writer.WriteLine(subtitle.Text);
                    writer.WriteLine();

                    index++;
                }
            }
        }

        public void SaveTranslation(string path, ICollection<Subtitles> subtitles)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                int index = 1;
                foreach (Subtitles subtitle in subtitles)
                {
                    writer.WriteLine(index);
                    writer.WriteLine($"{FormatTime(subtitle.ShowTime)} --> {FormatTime(subtitle.HideTime)}");
                    writer.WriteLine(subtitle.Translation);
                    writer.WriteLine();

                    index++;
                }
            }
        }

        private static string FormatTime(TimeSpan timeSpan)
        {
            return timeSpan.ToString(@"hh\:mm\:ss\,fff");
        }
    }
}
