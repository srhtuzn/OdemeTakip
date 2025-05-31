using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OdemeTakip.Desktop.Converters
{
    public class DurumRenkConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? Brushes.ForestGreen : Brushes.OrangeRed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
