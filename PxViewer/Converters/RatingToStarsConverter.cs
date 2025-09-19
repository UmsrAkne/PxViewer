using System;
using System.Globalization;
using System.Windows.Data;
using PxViewer.Models;

namespace PxViewer.Converters
{
    public class RatingToStarsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Rating rating)
            {
                return string.Empty;
            }

            var stars = (int)rating;

            if (stars == 0)
            {
                return string.Empty;
            }

            return new string('★', stars) + new string('☆', 5 - stars);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}