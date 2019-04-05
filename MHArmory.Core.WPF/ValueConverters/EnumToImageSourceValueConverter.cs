using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Resources;
using MHArmory.Core.DataStructures;
using MHArmory.Core.WPF.ScalableVectorGraphics;

namespace MHArmory.Core.WPF.ValueConverters
{
    public class EquipmentEnumToImageSourceValueConverter : EnumToImageSourceValueConverter
    {
        public EquipmentEnumToImageSourceValueConverter()
            : base("Icons/Equipments")
        {
        }
    }

    public class WeaponEnumToImageSourceValueConverter : EnumToImageSourceValueConverter
    {
        public WeaponEnumToImageSourceValueConverter()
            : base("Icons/Weapons")
        {
        }
    }

    public class ElementEnumToImageSourceValueConverter : EnumToImageSourceValueConverter
    {
        public ElementEnumToImageSourceValueConverter()
            : base("Icons/Elements")
        {
        }
    }

    public abstract class EnumToImageSourceValueConverter : IValueConverter
    {
        private readonly string relativeDirectory;

        protected EnumToImageSourceValueConverter(string relativeDirectory)
        {
            this.relativeDirectory = relativeDirectory;
        }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            string strType = value.ToString();

            if (parameter is string strParam && int.TryParse(strParam, out int rasterSize))
                return RasterizedImageContainer.GetRasterizedImage(rasterSize, $"{relativeDirectory}/{strType}.svg");

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
