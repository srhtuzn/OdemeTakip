// OdemeTakip.Desktop.Converters/OdemeDurumuToButtonTextConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace OdemeTakip.Desktop.Converters
{
    public class OdemeDurumuToButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPaid)
            {
                return isPaid ? "Geri Al" : "Öde";
            }
            return "İşlem";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}