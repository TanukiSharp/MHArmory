using System;
using System.Globalization;
using System.Windows.Data;
using MHArmory.Search.Contracts;

namespace MHArmory.Core.WPF.ValueConverters
{
    public class CombinationPerSecondValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SearchMetrics searchMetrics)
            {
                if (searchMetrics.TimeElapsed <= 0)
                    return "-";

                double cps = searchMetrics.CombinationCount / (searchMetrics.TimeElapsed / 1000.0);
                return cps.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "'" });
            }

            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
