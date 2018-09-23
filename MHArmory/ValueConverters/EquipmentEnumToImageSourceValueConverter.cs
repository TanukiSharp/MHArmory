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
using MHArmory.ScalableVectorGraphics;

namespace MHArmory.ValueConverters
{
    public class EquipmentEnumToImageSourceValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            string strType = value.ToString();

            if (parameter is string strParam && int.TryParse(strParam, out int rasterSize))
                return RasterizedImageContainer.GetRasterizedImage(rasterSize, $"Icons/Equipments/{strType}.svg");

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
