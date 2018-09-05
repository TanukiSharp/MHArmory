using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.ServiceContracts
{
    public enum MessageBoxButton
    {
        OK = 0,
        OKCancel = 1,
        YesNoCancel = 3,
        YesNo = 4
    }

    public enum MessageBoxImage
    {
        None = 0,
        Hand = 16,
        Stop = 16,
        Error = 16,
        Question = 32,
        Exclamation = 48,
        Warning = 48,
        Asterisk = 64,
        Information = 64
    }

    public enum MessageBoxResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Yes = 6,
        No = 7
    }

    public class MessageBoxServiceOptions
    {
        public string MessageBoxText { get; set; }
        public string Title { get; set; }
        public MessageBoxButton Buttons { get; set; }
        public MessageBoxImage Icon { get; set; }
        public MessageBoxResult DefaultResult { get; set; }
        public IntPtr Owner { get; set; }
    }

    public interface IMessageBoxService
    {
        MessageBoxResult Show(MessageBoxServiceOptions options);
    }
}
