using P2PKaraokeSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace P2PKaraokeSystem.View.Converter
{
    public class PlaylistVisiblityConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var video = (VideoDatabase.Video)value[0];
            var keywordText = value[1].ToString();

            if (keywordText.Length == 0)
            {
                return Visibility.Visible;
            }

            var keywords = keywordText.Split(' ');

            foreach (string keyword in keywords)
            {
                string searchKeyword = keyword.Trim().ToLower();

                if (searchKeyword.Length <= 0)
                {
                    continue;
                }

                if (video.Title.ToLower().Contains(searchKeyword)
                    || video.Performer.Name.ToLower().Contains(searchKeyword))
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
