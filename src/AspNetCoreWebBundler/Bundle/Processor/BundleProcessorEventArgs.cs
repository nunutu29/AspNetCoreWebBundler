using System;

namespace AspNetCoreWebBundler
{
    internal class BundleProcessorEventArgs : EventArgs
    {
        public BundleProcessorEventArgs(Bundle bundle, string baseFolder, bool containsChanges)
        {
            Bundle = bundle;
            BaseFolder = baseFolder;
            ContainsChanges = containsChanges;
        }

        public Bundle Bundle { get; set; }
        public string BaseFolder { get; set; }
        public bool ContainsChanges { get; set; }
    }
}
