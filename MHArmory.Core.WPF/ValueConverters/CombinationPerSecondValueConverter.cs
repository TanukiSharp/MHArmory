using MHArmory.Search.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

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

    public class SearchSpeedScoreValueConverter : IValueConverter
    {
        private static readonly double totalClockSpeed;

        static SearchSpeedScoreValueConverter()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                foreach (ManagementBaseObject item in searcher.Get())
                {
                    uint coresCount = (uint)item["NumberOfCores"];
                    uint clockSpeed = (uint)item["MaxClockSpeed"];

                    totalClockSpeed += coresCount * clockSpeed;

                    item.Dispose();
                }
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SearchMetrics searchMetrics)
            {
                double time = searchMetrics.TimeElapsed / 1000.0;

                if (time <= 1e-3d)
                    return "-";

                double cps = searchMetrics.CombinationCount / time;
                double score = cps / totalClockSpeed;

                return $"{score:f3} / {1_000_000d / score:f1}";
            }

            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
