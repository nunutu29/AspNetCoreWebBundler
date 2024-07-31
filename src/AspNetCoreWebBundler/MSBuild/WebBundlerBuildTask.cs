using System.IO;
using Microsoft.Build.Utilities;

namespace AspNetCoreWebBundler
{
    /// <summary>
    /// An MSBuild task for running web compilers on a given config file.
    /// </summary>
    public class WebBundlerBuildTask : Task
    {
        private bool _isSuccessful = true;

        /// <summary>
        /// The path of the configuration file.
        /// </summary>
        public string ConfigurationFile { get; set; }

        /// <summary>
        /// Execute the Task
        /// </summary>
        public override bool Execute()
        {
            var logger = new TaskLogWrapper(Log);
            var configFile = new FileInfo(ConfigurationFile);

            logger.LogInformation("Begin processing " + configFile.Name);

            if (!configFile.Exists)
            {
                logger.LogWarning(configFile.FullName + " does not exist");
                return true;
            }

            var processor = new BundleProcessor();

            processor.Processing += (s, e) =>
            {
                logger.LogDebug($"Processing {e.Bundle.Dest}");
                FileHelper.RemoveReadonly(e.Bundle.AbsoluteOutputFile);
            };

            processor.AfterBundling += (sender, e) =>
            {
                logger.LogDebug("Bundled " + e.Bundle.AbsoluteOutputFile);
            };

            processor.BeforeWritingMinFile += (s, e) =>
            {
                FileHelper.RemoveReadonly(e.ResultFile);
            };

            processor.AfterWritingMinFile += (sender, e) =>
            {
                logger.LogDebug("Minified " + e.ResultFile);
            };

            processor.ErrorMinifyingFile += (sender, e) =>
            {
                if (e.Result == null || !e.Result.HasErrors)
                {
                    return;
                }

                _isSuccessful = false;

                foreach (var error in e.Result.Errors)
                {
                    logger.LogError(error);
                }
            };

            processor.BeforeWritingGzipFile += (s, e) =>
            {
                FileHelper.RemoveReadonly(e.ResultFile);
            };

            processor.AfterWritingGzipFile += (s, e) =>
            {
                logger.LogDebug("GZipped " + e.ResultFile);
            };

            processor.BeforeWritingSourceMap += (s, e) =>
            {
                FileHelper.RemoveReadonly(e.ResultFile);
            };

            processor.AfterWritingSourceMap += (sender, e) =>
            {
                logger.LogDebug("SourceMap " + e.ResultFile);
            };

            processor.MinificationSkipped += (s, e) =>
            {
                logger.LogDebug("No changes, skipping minification of " + e.Bundle.AbsoluteOutputFile);
            };

            processor.Process(configFile.FullName);

            logger.LogInformation("Done processing " + configFile.Name);

            return _isSuccessful;
        }
    }
}
