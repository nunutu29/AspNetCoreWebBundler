using System;
using System.IO;
using System.Text;

namespace AspNetCoreWebBundler
{
    /// <summary>
    /// Helper class for file interactions.
    /// </summary>
    internal static class FileHelper
    {
        private const string FileProtocol = "file:///";


        /// <summary>
        /// Generates the filename for a minified version of the given file.
        /// If the filename already contains ".min.", it is returned unchanged.
        /// </summary>
        /// <param name="fileName">The original filename to be minified.</param>
        /// <returns>
        /// The filename with ".min" inserted before the extension if the original filename is not already minified.
        /// If the original filename is already minified, the same filename is returned.
        /// </returns>
        public static string GetMinFileName(string fileName)
        {
            // Check if the file is already minified by looking for ".min." in the file name
            if (Path.GetFileName(fileName).IndexOf(".min.", StringComparison.OrdinalIgnoreCase) > 0)
            {
                return fileName;
            }

            // extract the extensions
            var ext = Path.GetExtension(fileName);

            // Insert ".min" before the file extension
            return fileName.Substring(0, fileName.LastIndexOf(ext, StringComparison.OrdinalIgnoreCase)) + ".min" + ext;
        }

        /// <summary>
        /// Removes the read-only attribute from the specified file if it exists and is marked as read-only.
        /// </summary>
        /// <param name="fileName">The path to the file from which to remove the read-only attribute.</param>
        public static void RemoveReadonly(string fileName)
        {
            var file = new FileInfo(fileName);
            
            // Check if the file exists and is marked as read-only
            if (file.Exists && file.IsReadOnly)
            {
                // Remove the read-only attribute to allow modifications
                file.IsReadOnly = false;
            }
        }

        /// <summary>
        /// Checks whether the content of the specified file has changed compared to the given new content.
        /// </summary>
        /// <param name="fileName">The path to the file to check.</param>
        /// <param name="newContent">The new content to compare against the file's current content.</param>
        /// <returns>True if the file does not exist or if its content differs from the specified new content; otherwise, false.</returns>
        public static bool HasContentChanged(string fileName, string newContent)
        {
            // Check if the file exists
            if (!File.Exists(fileName))
            {
                // File does not exist, so content is considered changed
                return true;
            }
            
            // Compare the new content with the existing content of the file
            return newContent != File.ReadAllText(fileName);
        }
        
        /// <summary>
        /// Creates the directory (unless it already exists) where the file should be saved.
        /// </summary>
        public static DirectoryInfo CreateParentDirectory(string fileName)
        {
           return Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);
        }

        /// <summary>
        /// Creates a new file, writes the specified string, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        public static void Write(string fileName, string content)
        {
            File.WriteAllText(fileName, content, new UTF8Encoding(false));
        }

    }
}
