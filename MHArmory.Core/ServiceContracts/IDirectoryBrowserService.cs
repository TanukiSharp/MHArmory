using System;

namespace MHArmory.Core.ServiceContracts
{
    public enum DialogResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7
    }

    public class DirectoryBrowserServiceOptions
    {
        public bool ShowNewFolderButton { get; set; }
        public string SelectedPath { get; set; }
        public string Description { get; set; }
        public IntPtr Owner { get; set; }
    }

    public interface IDirectoryBrowserService
    {
        DialogResult ShowDialog(DirectoryBrowserServiceOptions options);
    }
}
