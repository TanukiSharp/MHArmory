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
using System.Windows.Shapes;
using MHArmory.ViewModels;
using Microsoft.Win32;

namespace MHArmory.Services
{
    public class SearchResultScreenshotService
    {
        public static readonly SearchResultScreenshotService Instance = new SearchResultScreenshotService();

        public BitmapSource RenderToImage(ArmorSetViewModel searchResult)
        {
            var root = new StackPanel
            {
                UseLayoutRounding = true,
                SnapsToDevicePixels = true,
                Orientation = Orientation.Horizontal,
            };

            root.Children.Add(new Rectangle
            {
                Fill = Brushes.Transparent,
                Width = 2.0,
                Height = 1.0
            });

            root.Children.Add(new ItemsControl
            {
                UseLayoutRounding = true,
                SnapsToDevicePixels = true,
                VerticalAlignment = VerticalAlignment.Top,
                ItemsSource = searchResult.DesiredAbilities.Select(x => new AbilityViewModel(x, null) { IsChecked = true }).ToArray(),
                ItemTemplate = (DataTemplate)App.Current.FindResource("SelectedAbilityView"),
                Margin = new Thickness(4.0),
                Focusable = false
            });

            root.Children.Add(new ContentControl
            {
                UseLayoutRounding = true,
                SnapsToDevicePixels = true,
                Content = searchResult,
                ContentTemplate = (DataTemplate)App.Current.FindResource("SearchResultArmorSetView")
            });

            return RenderUtils.RenderToImage(root);
        }

        public void SaveToFile(Func<BitmapSource> bitmapSource)
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
