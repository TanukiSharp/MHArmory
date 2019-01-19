using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MHArmory.Core.WPF.Controls
{
    public class SharpnessRenderer : FrameworkElement
    {
        public SharpnessRenderer()
        {
            ClipToBounds = true;
        }

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            nameof(Background),
            typeof(Brush),
            typeof(SharpnessRenderer),
            new PropertyMetadata(Brushes.Gray)
        );

        public int SharpnessRank
        {
            get { return (int)GetValue(SharpnessRankProperty); }
            set { SetValue(SharpnessRankProperty, value); }
        }

        public static readonly DependencyProperty SharpnessRankProperty = DependencyProperty.Register(
            nameof(SharpnessRank),
            typeof(int),
            typeof(SharpnessRenderer),
            new PropertyMetadata(0, OnSharpnessParametersChanged)
        );

        public IList<int[]> SharpnessLevels
        {
            get { return (IList<int[]>)GetValue(SharpnessLevelsProperty); }
            set { SetValue(SharpnessLevelsProperty, value); }
        }

        public static readonly DependencyProperty SharpnessLevelsProperty = DependencyProperty.Register(
            nameof(SharpnessLevels),
            typeof(IList<int[]>),
            typeof(SharpnessRenderer),
            new PropertyMetadata(OnSharpnessParametersChanged)
        );

        private static void OnSharpnessParametersChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((SharpnessRenderer)sender).InvalidateVisual();
        }

        private static readonly Brush[] SharpnessBrushes = new SolidColorBrush[]
        {
            new SolidColorBrush(Color.FromRgb(255, 0, 0)), // red
            new SolidColorBrush(Color.FromRgb(255, 140, 0)), // orange
            new SolidColorBrush(Color.FromRgb(255, 255, 0)), // yellow
            new SolidColorBrush(Color.FromRgb(0, 255, 0)), // green
            new SolidColorBrush(Color.FromRgb(0, 128, 255)), // blue
            new SolidColorBrush(Color.FromRgb(255, 255, 255)), // white
        };

        static SharpnessRenderer()
        {
            foreach (Brush brush in SharpnessBrushes)
            {
                if (brush.CanFreeze)
                    brush.Freeze();
            }
        }

        private const int MaxSharpness = 400;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            int localSharpnessRank = SharpnessRank;
            IList<int[]> localSharpnessLevels = SharpnessLevels;

            if (localSharpnessLevels == null || localSharpnessRank < 0 || localSharpnessRank > localSharpnessLevels.Count - 1)
                return;

            int[] sharpnessValues = localSharpnessLevels[localSharpnessRank];

            double localWidth = Width;
            double localHeight = Height;

            double left = 0.0;

            for (int i = 0; i < sharpnessValues.Length; i++)
            {
                double width = Math.Round(localWidth * sharpnessValues[i] / MaxSharpness);
                drawingContext.DrawRectangle(SharpnessBrushes[i], null, new Rect(left, 0.0, width, localHeight));
                left += width;
            }

            if (left < localWidth)
                drawingContext.DrawRectangle(Background, null, new Rect(left, 0.0, localWidth - left, localHeight));
        }
    }
}
