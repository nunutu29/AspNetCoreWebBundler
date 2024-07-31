using System;
using System.IO;
using System.Linq;

namespace AspNetCoreWebBundler
{
    internal class BundleCleaner
    {
        public event EventHandler<BundleCleanerEventArgs> Deleted;
        public event EventHandler<BundleCleanerErrorEventArgs> Error;

        public bool Clean(string configFile)
        {
            if (BundleConfig.TryParse(configFile, out var bundles))
            {
                foreach (var bundle in bundles)
                {
                    foreach (var inner in bundle.Expand())
                    {
                        CleanBundle(inner);
                    }
                }

                return true;
            }

            return false;
        }

        private void CleanBundle(Bundle bundle)
        {
            if (!bundle.AbsoluteInputFiles.Contains(bundle.AbsoluteOutputFile, StringComparer.OrdinalIgnoreCase))
            {
                DeleteFile(bundle, bundle.AbsoluteOutputFile);
            }

            DeleteFile(bundle, bundle.AbsoluteOutputFile + ".gz");

            var minFile = FileHelper.GetMinFileName(bundle.AbsoluteOutputFile);
            
            DeleteFile(bundle, minFile);
            DeleteFile(bundle, minFile + ".map");
            DeleteFile(bundle, minFile + ".gz");
        }

        private void DeleteFile(Bundle bundle, string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    FileHelper.RemoveReadonly(file);
                    File.Delete(file);

                    OnDeleted(bundle, file);
                }
            }
            catch (Exception ex)
            {
                OnError(bundle, file, ex);
            }
        }

        protected void OnDeleted(Bundle bundle, string deletedFileName)
        {
            Deleted?.Invoke(this, new BundleCleanerEventArgs(bundle, deletedFileName));
        }

        protected void OnError(Bundle bundle, string deletedFileName, Exception exception)
        {
            Error?.Invoke(this, new BundleCleanerErrorEventArgs(bundle, deletedFileName, exception));
        }
    }
}