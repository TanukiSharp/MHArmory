using System;
using System.Windows;
using System.Windows.Interop;
using MHArmory.Core.ServiceContracts;

namespace MHArmory.Services
{
    public class MessageBoxService : IMessageBoxService
    {
        public Core.ServiceContracts.MessageBoxResult Show(MessageBoxServiceOptions options)
        {
            Window ownerWindow = null;

            if (options != null && options.Owner != IntPtr.Zero)
                ownerWindow = (Window)HwndSource.FromHwnd(options.Owner).RootVisual;

            return (Core.ServiceContracts.MessageBoxResult)MessageBox.Show(
                ownerWindow ?? Application.Current.MainWindow,
                options?.MessageBoxText ?? string.Empty,
                options?.Title ?? string.Empty,
                (System.Windows.MessageBoxButton)(options?.Buttons ?? Core.ServiceContracts.MessageBoxButton.OK),
                (System.Windows.MessageBoxImage)(options?.Icon ?? Core.ServiceContracts.MessageBoxImage.None),
                (System.Windows.MessageBoxResult)(options?.DefaultResult ?? Core.ServiceContracts.MessageBoxResult.None)
            );
        }
    }
}
