// OdemeTakip.Desktop.Converters/DurumRenkConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OdemeTakip.Desktop.Converters
{
    public class DurumRenkConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? Brushes.ForestGreen : Brushes.OrangeRed; // Ödendi ise yeşil, bekleniyor ise kırmızı
            }
            return Brushes.Gray; // Varsayılan veya bilinmeyen durum
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}