using System;
using System.Globalization;
using System.Windows.Data;

namespace MHArmory.ValueConverters
{
    public class NumToSeparatedStringValueConverter : IValueConverter
    {
        private static readonly NumberFormatInfo nfi = new NumberFormatInfo
        {
            NumberGroupSeparator = "'"
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ChangeType(value, typeof(string), nfi);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
