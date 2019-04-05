using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MHArmory.Core.WPF.ScalableVectorGraphics
{
    public class Rasterizer
    {
        public static ImageSource Render(int width, int height, VectorGraphicsInfo vectorGraphicsInfo)
        {
            DpiScale dpiInfo = VisualTreeHelper.GetDpi(Application.Current.MainWindow);
            PixelFormat pixelFormat = PixelFormats.Pbgra32;

            var x = new RenderTargetBitmap(width, height, dpiInfo.PixelsPerInchX / dpiInfo.DpiScaleX, dpiInfo.PixelsPerInchY / dpiInfo.DpiScaleY, pixelFormat);

            var canvas = new Canvas
            {
                Width = vectorGraphicsInfo.Viewbox.Width,
                Height = vectorGraphicsInfo.Viewbox.Height,
                SnapsToDevicePixels = false,
                UseLayoutRounding = true
            };

            foreach (Path path in vectorGraphicsInfo.Paths)
                canvas.Children.Add(path);

            var viewbox = new Viewbox
            {
                Stretch = Stretch.Fill,
                Width = width,
                Height = height,
                SnapsToDevicePixels = false,
                UseLayoutRounding = true,
                Child = canvas
            };

            var size = new Size(width, height);

            viewbox.Measure(size);
            viewbox.Arrange(new Rect(size));

            x.Render(viewbox);

            x.Freeze();

            return x;
        }
    }
}
