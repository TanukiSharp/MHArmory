using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MHArmory.Core.DataStructures;

namespace MHArmory.Core.WPF.ValueConverters
{
    public class ElementNameValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ElementType e)
            {
                switch (e)
                {
                    case ElementType.Fire: return "Fire";
                    case ElementType.Water: return "Water";
                    case ElementType.Thunder: return "Thunder";
                    case ElementType.Ice: return "Ice";
                    case ElementType.Dragon: return "Dragon";
                    case ElementType.Poison: return "Poison";
                    case ElementType.Sleep: return "Sleep";
                    case ElementType.Paralysis: return "Paralysis";
                    case ElementType.Blast: return "Blast";
                }
            }

            return "(unknown)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
