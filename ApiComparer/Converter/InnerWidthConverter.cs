using System;
using System.Globalization;
using System.Windows.Data;

namespace ApiComparer.Converter
{
    public class InnerWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                var width = (double) value;
                return width - 17 < 0 ? 0 : width - 17;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}