using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace AspNetCoreWebBundler
{
    internal static class BundleConfig
    {
        public static bool TryParse(string configFile, out IEnumerable<Bundle> bundles)
        {
            try
            {
                if (File.Exists(configFile))
                {
                    var content = File.ReadAllText(configFile);

                    bundles = JsonConvert.DeserializeObject<Bundle[]>(content);
                    bundles = bundles.Where(bundle => bundle.Src.Count > 0 && !string.IsNullOrEmpty(bundle.Dest));

                    foreach (var bundle in bundles)
                    {
                        bundle.ConfigFile = configFile;
                    }

                    return true;
                }

                bundles = Enumerable.Empty<Bundle>();
            }
            catch
            {
                bundles = null;
            }

            return false;
        }
    }
}
