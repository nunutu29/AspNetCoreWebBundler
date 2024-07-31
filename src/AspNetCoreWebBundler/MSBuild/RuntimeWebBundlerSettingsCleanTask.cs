using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace AspNetCoreWebBundler
{
    /// <summary>
    ///  An MSBuild task for cleaning runtime settins during DEBUG.
    /// </summary>
    public class RuntimeWebBundlerSettingsCleanTask : Task
    {
        /// <summary>
        /// The path where to write the generated code.
        /// </summary>
        [Required]
        public string OutputFile { get; set; }

        /// <summary>
        /// Execute the Task
        /// </summary>
        public override bool Execute()
        {
            var logger = new TaskLogWrapper(Log);

            try
            {
                var file = new FileInfo(OutputFile);

                logger.LogInformation("Begin cleaning " + file.Name);

                FileHelper.RemoveReadonly(file.FullName);
                file.Delete();

                logger.LogInformation("Done cleaning " + file.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                return false;
            }

            return true;
        }
    }
}