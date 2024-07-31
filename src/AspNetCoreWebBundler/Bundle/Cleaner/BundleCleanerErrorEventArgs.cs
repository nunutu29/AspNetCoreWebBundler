using System;

namespace AspNetCoreWebBundler
{
    internal class BundleCleanerErrorEventArgs(Bundle bundle, string fileName, Exception exception) : BundleCleanerEventArgs(bundle, fileName)
    {
        public Exception Exception { get; set; } = exception;
    }
}