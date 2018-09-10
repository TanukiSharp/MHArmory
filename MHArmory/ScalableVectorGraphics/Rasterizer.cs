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

namespace MHArmory.ScalableVectorGraphics
{
    public class Rasterizer
    {
        public static ImageSource Render(int width, int height, VectorGraphicsInfo vectorGraphicsInfo)
        {
            DpiScale dpiInfo = VisualTreeHelper.GetDpi(App.Current.MainWindow);
            PixelFormat pixelFormat = PixelFormats.Pbgra32;

            var x = new RenderTargetBitmap(width, height, dpiInfo.PixelsPerInchX, dpiInfo.PixelsPerInchY, pixelFormat);

            var canvas = new Canvas
            {
                Width = vectorGraphicsInfo.Viewbox.Width,
                Height = vectorGraphicsInfo.Viewbox.Height
            };

            foreach (Path path in vectorGraphicsInfo.Paths)
                canvas.Children.Add(path);

            var viewbox = new Viewbox
            {
                Stretch = Stretch.Fill,
                Width = width,
                Height = height,
                Child = canvas
            };

            viewbox.Measure(new Size(width, height));
            viewbox.Arrange(new Rect(new Size(width, height)));

            x.Render(viewbox);

            x.Freeze();

            return x;
        }
    }
}
