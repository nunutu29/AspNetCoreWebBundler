using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace AspNetCoreWebBundler
{
    /// <summary>
    ///  An MSBuild task for running the cleaning on a given config file.
    /// </summary>
    public class WebBundlerCleanTask : Task
    {
        /// <summary>
        /// The path of the configuration file.
        /// </summary>
        [Required]
        public string ConfigurationFile { get; set; }

        /// <summary>
        /// Execute the Task
        /// </summary>
        public override bool Execute()
        {
            var logger = new TaskLogWrapper(Log);
            var configFile = new FileInfo(ConfigurationFile);

            logger.LogInformation("Cleaning output from " + configFile.Name);

            if (!configFile.Exists)
            {
                logger.LogWarning(configFile.FullName + " does not exist");
                return true;
            }

            var cleaner = new BundleCleaner();

            cleaner.Deleted += (_, e) =>
            {
                logger.LogDebug("Deleted " + e.FileName);
            };

            cleaner.Error += (_, e) =>
            {
                logger.LogError("Could not delete file " + e.FileName + ". Error message: " + e.Exception.Message);
            };

            var done = cleaner.Clean(configFile.FullName);

            if (done)
            {
                logger.LogInformation("Done cleaning output file from " + configFile.Name);
                return true;
            }

            logger.LogWarning($"There was an error reading {configFile.Name}");
            return false;
        }
    }
}
