using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MHArmory.Core.WPF.ValueConverters
{
    public class MillisecondsToTimeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int ms)
            {
                if (ms >= 60000)
                    return $"{ms / 60000}m{(ms % 60000) / 1000}s";
                else
                    return $"{ms / 1000.0:f2}s";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
