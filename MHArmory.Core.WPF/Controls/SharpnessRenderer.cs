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

        public double FullSharpnessHeight
        {
            get { return (double)GetValue(FullSharpnessHeightProperty); }
            set { SetValue(FullSharpnessHeightProperty, value); }
        }

        public static readonly DependencyProperty FullSharpnessHeightProperty = DependencyProperty.Register(
            nameof(FullSharpnessHeight),
            typeof(double),
            typeof(SharpnessRenderer),
            new PropertyMetadata(0.0)
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

        public IList<ushort[]> SharpnessLevels
        {
            get { return (IList<ushort[]>)GetValue(SharpnessLevelsProperty); }
            set { SetValue(SharpnessLevelsProperty, value); }
        }

        public static readonly DependencyProperty SharpnessLevelsProperty = DependencyProperty.Register(
            nameof(SharpnessLevels),
            typeof(IList<ushort[]>),
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
            new SolidColorBrush(Color.FromRgb(200, 50, 200)), // purple
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
            IList<ushort[]> localSharpnessLevels = SharpnessLevels;

            if (localSharpnessLevels == null || localSharpnessRank < 0)
                return;

            ushort[] sharpnessValues = localSharpnessLevels[Math.Min(localSharpnessRank, localSharpnessLevels.Count - 1)];

            double localWidth = Width;
            double localHeight = Height;

            double localFullSharpnessHeight = FullSharpnessHeight;
            double currentSharpnessHeight = localHeight - localFullSharpnessHeight;

            double left = 0.0;

            for (int i = 0; i < sharpnessValues.Length; i++)
            {
                double width = Math.Round(localWidth * sharpnessValues[i] / MaxSharpness);
                drawingContext.DrawRectangle(SharpnessBrushes[i], null, new Rect(left, 0.0, width, currentSharpnessHeight));
                left += width;
            }

            if (left < localWidth)
                drawingContext.DrawRectangle(Background, null, new Rect(left, 0.0, localWidth - left, localHeight));

            left = 0.0;
            sharpnessValues = localSharpnessLevels[localSharpnessLevels.Count - 1];

            for (int i = 0; i < sharpnessValues.Length; i++)
            {
                double width = Math.Round(localWidth * sharpnessValues[i] / MaxSharpness);
                drawingContext.DrawRectangle(SharpnessBrushes[i], null, new Rect(left, currentSharpnessHeight, width, localFullSharpnessHeight));
                left += width;
            }
        }
    }
}
