using System;
using System.Globalization;
using System.Windows.Data;

namespace OdemeTakip.Desktop.Converters
{
    public class OdemeDurumuToButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool odenmeDurumu)
            {
                return odenmeDurumu ? "Geri Al" : "Öde";
            }
            return "Öde";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
