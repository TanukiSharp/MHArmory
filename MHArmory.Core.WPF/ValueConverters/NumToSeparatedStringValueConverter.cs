using System;
using System.Globalization;
using System.Windows.Data;

namespace MHArmory.Core.WPF.ValueConverters
{
    public class NumToSeparatedStringValueConverter : IValueConverter
    {
        private static readonly NumberFormatInfo nfi = new NumberFormatInfo
        {
            NumberGroupSeparator = "'"
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int num)
                return num.ToString("N0", nfi);

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
