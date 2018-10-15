using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MHArmory.Services
{
    public interface IRenderService
    {
        BitmapSource RenderToImage(object item, string dataTemplateName);
        BitmapSource RenderToImage(UIElement element);
    }

    public class RenderService : IRenderService
    {
        public BitmapSource RenderToImage(object item, string dataTemplateName)
        {
            var contentControl = new ContentControl
            {
                UseLayoutRounding = true,
                SnapsToDevicePixels = true,
                Content = item,
                ContentTemplate = (DataTemplate)App.Current.FindResource(dataTemplateName)
            };

            return RenderToImage(contentControl);
        }

        public BitmapSource RenderToImage(UIElement element)
        {
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(new Point(), element.DesiredSize));

            // call to UpdateLayout is required to force children to be rendered too
            element.UpdateLayout();

            var renderTarget = new RenderTargetBitmap((int)element.DesiredSize.Width, (int)element.DesiredSize.Height, 96.0, 96.0, PixelFormats.Pbgra32);

            var drawingVisual = new DrawingVisual();

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                var rect = new Rect(new Point(0, 0), new Point(element.DesiredSize.Width, element.DesiredSize.Height));

                drawingContext.DrawRectangle(Brushes.White, null, rect);
                drawingContext.DrawRectangle(new VisualBrush(element), null, rect);
            }

            renderTarget.Render(drawingVisual);

            return renderTarget;
        }
    }
}
