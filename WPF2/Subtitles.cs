using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF2
{
    public class Subtitles: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private TimeSpan _showTime;
        private TimeSpan _hideTime;
        private string? _text;
        private string? _translation;
        public TimeSpan ShowTime
        {
            get { return _showTime; }
            set
            {
                _showTime = value;
                NotifyPropertyChanged("ShowTime");
                NotifyPropertyChanged("Duration");
            }
        }
        public TimeSpan HideTime
        {
            get { return _hideTime; }
            set
            {
                _hideTime = value;
                NotifyPropertyChanged("HideTime");
                NotifyPropertyChanged("Duration");
            }
        }
        public TimeSpan Duration
        {
            get 
            {
                if (ShowTime > HideTime) return TimeSpan.Zero;
                return HideTime - ShowTime; 
            }
            set 
            { 
               
                HideTime = ShowTime + value;
            }
        }
        public string? Text
        {
            get { return _text; }
            set
            {
                _text = value;
                NotifyPropertyChanged("Text");
            }
        }
        public string? Translation
        {
            get { return _translation; }
            set
            {
                _translation = value;
                NotifyPropertyChanged("Translation");
            }
        }
        public Subtitles(TimeSpan showTime, TimeSpan hideTime, string text, string translation)
        {
            this.ShowTime= showTime;
            this.HideTime= hideTime;
            this.Text= text;
            this.Translation = translation;
        }
        public Subtitles()
        {
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this,
                new PropertyChangedEventArgs(propertyName));
        }

    }
}
