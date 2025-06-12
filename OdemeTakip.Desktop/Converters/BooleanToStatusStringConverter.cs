// OdemeTakip.Desktop.Converters/BooleanToStatusStringConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace OdemeTakip.Desktop.Converters
{
    public class BooleanToStatusStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? "Aktif" : "Pasif";
            }
            return "Bilinmiyor";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}