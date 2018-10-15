using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace MHArmory
{
    public static class RenderUtils
    {
        public static BitmapSource RenderToImage(object item, string dataTemplateName)
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

        public static BitmapSource RenderToImage(UIElement element)
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

        public static void RenderToClipboard(object item, string dataTemplateName)
        {
            Clipboard.SetImage(RenderToImage(item, dataTemplateName));
        }

        public static void RenderToClipboard(UIElement element)
        {
            Clipboard.SetImage(RenderToImage(element));
        }

        public static void RenderToFile(object item, string dataTemplateName)
        {
            RenderToFile(() => RenderToImage(item, dataTemplateName));
        }

        public static void RenderToFile(UIElement element)
        {
            RenderToFile(() => RenderToImage(element));
        }

        public static void RenderToFile(Func<BitmapSource> bitmapSource)
        {
            var saveFileDialog = new SaveFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                Filter = "PNG Files (*.png)|*.png|All Files (*.*)|*.*",
                InitialDirectory = AppContext.BaseDirectory,
                OverwritePrompt = true,
                Title = "Save screenshot or armor set search result"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource()));

            using (FileStream fs = File.OpenWrite(saveFileDialog.FileName))
                encoder.Save(fs);
        }
    }
}
