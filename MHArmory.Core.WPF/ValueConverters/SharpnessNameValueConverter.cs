using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MHArmory.Core.WPF.ValueConverters
{
    public class SharpnessNameValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int s)
            {
                switch (s)
                {
                    case 0: return "Base sharpness";
                    case 1: return "Sharpness +1";
                    case 2: return "Sharpness +2";
                    case 3: return "Sharpness +3";
                    case 4: return "Sharpness +4";
                    case 5: return "Sharpness +5";
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
