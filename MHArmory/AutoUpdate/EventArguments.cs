using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.AutoUpdate
{
    public class NewVersionEventArgs : EventArgs
    {
        public Version NewVersion { get; }
        public Uri DownloadUrl { get; }

        public NewVersionEventArgs(Version newVersion, Uri downloadUrl)
        {
            NewVersion = newVersion;
            DownloadUrl = downloadUrl;
        }
    }
}
