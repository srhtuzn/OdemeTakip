using System;
using System.Globalization;
using System.Windows.Data;

namespace OdemeTakip.Desktop.Converters
{
    public class DurumYaziConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? "✓ Ödendi" : "⏳ Bekliyor";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
