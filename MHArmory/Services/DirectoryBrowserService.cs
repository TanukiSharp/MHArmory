// The code bellow requires System.Windows.Forms depencies

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using MHArmory.Core.ServiceContracts;

//namespace MHArmory.Services
//{
//    public class DirectoryBrowserService : IDirectoryBrowserService
//    {
//        private class Win32Window : IWin32Window
//        {
//            public IntPtr Handle { get; }

//            public Win32Window(IntPtr handle)
//            {
//                Handle = handle;
//            }
//        }

//        public Core.ServiceContracts.DialogResult ShowDialog(DirectoryBrowserServiceOptions options)
//        {
//            var dialog = new FolderBrowserDialog
//            {
//                Description = options?.Description ?? string.Empty,
//                SelectedPath = options?.SelectedPath,
//                ShowNewFolderButton = options?.ShowNewFolderButton ?? true
//            };

//            Core.ServiceContracts.DialogResult result;

//            if (options != null && options.Owner != IntPtr.Zero)
//                result = (Core.ServiceContracts.DialogResult)dialog.ShowDialog(new Win32Window(options.Owner));
//            else
//                result = (Core.ServiceContracts.DialogResult)dialog.ShowDialog();

//            options.SelectedPath = dialog.SelectedPath;

//            return result;
//        }
//    }
//}
