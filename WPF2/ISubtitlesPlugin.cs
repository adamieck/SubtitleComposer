using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF2
{
    public interface ISubtitlesPlugin
    {
        string Name { get; }
        string Extension { get; }
        ICollection<Subtitles> Load(string path);
        void Save(string path, ICollection<Subtitles> subtitles);
        void SaveTranslation(string path, ICollection<Subtitles> subtitles);
    }
}
