using System;

namespace AspNetCoreWebBundler
{
    internal class BundleCleanerEventArgs(Bundle bundle, string fileName) : EventArgs
    {
        public Bundle Bundle { get; set; } = bundle;
        public string FileName { get; set; } = fileName;
    }
}