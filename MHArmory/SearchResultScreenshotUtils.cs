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

namespace MHArmory
{
    public static class SearchResultScreenshotUtils
    {
        public static BitmapSource RenderToImage(ArmorSetViewModel searchResult, IEnumerable<int> weaponSlots)
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

            var weaponSlotsAndAbilities = new StackPanel
            {
                UseLayoutRounding = true,
                SnapsToDevicePixels = true,
            };

            var weaponSlotsPanel = new StackPanel
            {
                UseLayoutRounding = true,
                SnapsToDevicePixels = true,
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(4.0),
            };

            weaponSlotsPanel.Children.Add(new TextBlock
            {
                Text = "Weapon slots: ",
                VerticalAlignment = VerticalAlignment.Center
            });

            var factoryPanel = new FrameworkElementFactory(typeof(StackPanel));
            factoryPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            if (weaponSlots.Any(x => x > 0))
            {
                weaponSlotsPanel.Children.Add(new ItemsControl
                {
                    ItemsSource = weaponSlots,
                    Height = 24.0,
                    VerticalAlignment = VerticalAlignment.Center,
                    ItemTemplate = (DataTemplate)App.Current.FindResource("SlotImageView"),
                    ItemsPanel = new ItemsPanelTemplate(factoryPanel)
                });
            }
            else
            {
                weaponSlotsPanel.Children.Add(new TextBlock
                {
                    Text = "none",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontStyle = FontStyles.Italic
                });
            }

            weaponSlotsAndAbilities.Children.Add(weaponSlotsPanel);

            weaponSlotsAndAbilities.Children.Add(new ItemsControl
            {
                UseLayoutRounding = true,
                SnapsToDevicePixels = true,
                VerticalAlignment = VerticalAlignment.Top,
                ItemsSource = searchResult.DesiredAbilities.Select(x => new AbilityViewModel(x, null) { IsChecked = true }).ToArray(),
                ItemTemplate = (DataTemplate)App.Current.FindResource("SelectedAbilityView"),
                Margin = new Thickness(4.0),
                Focusable = false
            });

            root.Children.Add(weaponSlotsAndAbilities);

            root.Children.Add(new ContentControl
            {
                UseLayoutRounding = true,
                SnapsToDevicePixels = true,
                Content = searchResult,
                ContentTemplate = (DataTemplate)App.Current.FindResource("SearchResultArmorSetView")
            });

            return RenderUtils.RenderToImage(root);
        }
    }
}
