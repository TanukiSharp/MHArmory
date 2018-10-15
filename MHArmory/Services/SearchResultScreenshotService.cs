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
    public interface ISearchResultScreenshotService
    {
        BitmapSource RenderToImage(ArmorSetViewModel searchResult);
    }

    public class SearchResultScreenshotService : ISearchResultScreenshotService
    {
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

            return ServicesContainer.GetService<IRenderService>().RenderToImage(root);
        }
    }
}
