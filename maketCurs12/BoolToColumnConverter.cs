using System;
using System.Globalization;
using System.Windows.Data;

namespace maketCurs12
{
    public class BoolToColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
                return 1;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int && (int)value == 1)
                return true;
            return false;
        }
    }
}