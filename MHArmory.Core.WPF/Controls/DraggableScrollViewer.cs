using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MHArmory.Core.WPF.Controls
{
    public class DraggableScrollViewer : ScrollViewer
    {
        private Point originMousePosition;
        private Point originPanelPosition;

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            originMousePosition = e.GetPosition(this);
            originPanelPosition = new Point(HorizontalOffset, VerticalOffset);

            // Very important to capture AFTER acquiring the position
            CaptureMouse();
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (IsMouseCaptured)
            {
                Point currentPosition = e.GetPosition(this);

                ScrollToHorizontalOffset(originPanelPosition.X + (originMousePosition.X - currentPosition.X));
                ScrollToVerticalOffset(originPanelPosition.Y + (originMousePosition.Y - currentPosition.Y));
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            ReleaseMouseCapture();
        }
    }
}
