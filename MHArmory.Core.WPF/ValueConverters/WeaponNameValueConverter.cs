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
    public class WeaponNameValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WeaponType w)
            {
                switch (w)
                {
                    case WeaponType.GreatSword: return "Great Sword";
                    case WeaponType.LongSword: return "Long Sword";
                    case WeaponType.SwordAndShield: return "Sword and Shield";
                    case WeaponType.DualBlades: return "Dual Blades";
                    case WeaponType.Hammer: return "Hammer";
                    case WeaponType.HuntingHorn: return "Hunting Horn";
                    case WeaponType.Lance: return "Lance";
                    case WeaponType.Gunlance: return "Gunlance";
                    case WeaponType.SwitchAxe: return "Switch Axe";
                    case WeaponType.ChargeBlade: return "Charge Blade";
                    case WeaponType.InsectGlaive: return "Insect Glaive";
                    case WeaponType.LightBowgun: return "Light Bowgun";
                    case WeaponType.HeavyBowgun: return "Heavy Bowgun";
                    case WeaponType.Bow: return "Bow";
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
