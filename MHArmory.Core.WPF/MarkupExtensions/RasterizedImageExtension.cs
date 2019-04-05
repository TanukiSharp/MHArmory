using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using MHArmory.Core.WPF.ScalableVectorGraphics;

namespace MHArmory.Core.WPF.MarkupExtensions
{
    public class RasterizedImageExtension : MarkupExtension
    {
        public string Location { get; set; }
        public int Size { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return RasterizedImageContainer.GetRasterizedImage(Size, Location);
        }
    }
}
