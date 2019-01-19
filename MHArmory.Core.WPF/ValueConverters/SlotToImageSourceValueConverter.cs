using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MHArmory.Core.WPF.ScalableVectorGraphics;

namespace MHArmory.Core.WPF.ValueConverters
{
    public class SlotToImageSourceValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int slotSize && slotSize >= 1 && slotSize <= 3 && parameter is string rasterSizeStr && int.TryParse(rasterSizeStr, out int rasterSize))
                return RasterizedImageContainer.GetRasterizedImage(rasterSize, $"Icons/Jewels/Jewel{slotSize}.svg");

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
