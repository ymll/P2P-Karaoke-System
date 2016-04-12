using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace P2PKaraokeSystem.View.Converter
{
    public class TimeStringConverter : IValueConverter
    {
        private string Convert(long lengthInMillisecond)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(lengthInMillisecond);
            return time.ToString(@"hh\:mm\:ss");
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var video = (VideoDatabase.Video)value;
            var lengthInMillisecond = video.LengthInMillisecond;
            return Convert(lengthInMillisecond);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
