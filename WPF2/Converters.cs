using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPF2
{
    class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                string ret;
                string format = @"h\:mm\:ss\.fff";


                if (timeSpan.TotalSeconds >= 1)
                    return timeSpan.ToString(format, culture).Trim('0', ':').TrimEnd('.');
                else if (timeSpan.TotalMilliseconds > 0)
                    return $"0.{timeSpan:FFF}";
                else
                    return $"0{timeSpan:FFF}";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = value as string;
            if (string.IsNullOrEmpty(s))
                return TimeSpan.Zero;
            TimeSpan result;
            string[] formats = { @"hh\:mm\:ss\.FFF", @"hh\:mm\:ss", @"m\:ss\.FFF", @"s\.F", @"s\:F", @"%s", @"m\:s\.F", @"m\:s", @"h\:m\:s\.F", @"h\:m\:s", @"mm\:ss\.FFF", @"s\.FFF"};
            if (TimeSpan.TryParseExact(s, formats, culture, out result))
            {
                return result;
            }
            return TimeSpan.Zero;
        }
    }

    class BooleanToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool val = (bool)value;
            if(val == true)
            {
                return new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);
            }
            else
                return new System.Windows.GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    
    public class CharacterCounter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value as string;
            if (val != null)
            {
                return val.Length;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
