using System;
using Microsoft.Build.Framework;

namespace AspNetCoreWebBundler
{
    internal class TaskLogWrapper
    {
        private readonly Microsoft.Build.Utilities.TaskLoggingHelper _taskLogger;
        private static readonly string Prefix = Constants.PackageName + ": ";

        public TaskLogWrapper(Microsoft.Build.Utilities.TaskLoggingHelper taskLogger)
        {
            _taskLogger = taskLogger;
        }

        public void LogDebug(string message)
        {
            _taskLogger.LogMessage(MessageImportance.Normal, Prefix + message);
        }

        public void LogInformation(string message)
        {
            _taskLogger.LogMessage(MessageImportance.High, Prefix + message);
        }

        public void LogWarning(string message)
        {
            _taskLogger.LogWarning(Prefix + message);
        }

        public void LogError(string message)
        {
            _taskLogger.LogError(Prefix + message);
        }

        public void LogError(Exception ex)
        {
            _taskLogger.LogErrorFromException(ex);
        }

        public void LogError(BundleUglifierError error)
        {
            _taskLogger.LogError(Constants.PackageName, "0", "", error.FileName, error.LineNumber, error.ColumnNumber, error.LineNumber, error.ColumnNumber, error.Message, null);
        }
    }
}