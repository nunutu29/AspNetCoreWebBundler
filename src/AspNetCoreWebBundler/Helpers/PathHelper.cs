using System;
using System.IO;

namespace AspNetCoreWebBundler;

internal static class PathHelper
{
    /// <summary>
    /// Determines whether the specified path is a directory.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the specified path is a directory; otherwise, False.</returns>
    public static bool IsDirectory(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

        // Check if the path ends with a directory separator character, indicating it's likely a directory
        if (path[path.Length - 1] == Path.DirectorySeparatorChar || path[path.Length - 1] == Path.AltDirectorySeparatorChar)
        {
            return true;
        }

        // Check if the path exists as a directory
        if (Directory.Exists(path))
        {
            return true;
        }

        // Default to false if none of the above conditions are met
        return false;
    }

    /// <summary>
    /// Finds the relative path between two files.
    /// </summary>
    /// <param name="relativeTo">Contains the directory that defines the start of the relative path.</param>
    /// <param name="path">Contains the path that defines the endpoint of the relative path.</param>
    public static string MakeRelative(string relativeTo, string path)
    {
        // https://stackoverflow.com/a/340454/4537709

        if (string.IsNullOrEmpty(relativeTo)) throw new ArgumentNullException(nameof(relativeTo));
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        
        var fromUri = new Uri("file:///" + relativeTo, UriKind.RelativeOrAbsolute);
        var toUri = new Uri("file:///" + path, UriKind.RelativeOrAbsolute);

        //if (fromUri.Scheme != toUri.Scheme)
        //{
        //    // path can't be made relative.
        //    return path;
        //} 

        var relativeUri = fromUri.MakeRelativeUri(toUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        //if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
        //{
        //    relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        //}

        //relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        return relativePath;
    }

    public static string ComputeBundleOuputPath(string baseDir, string namePattern, bool baseRecursive, string srcFile, string srcPattern)
    {
        var srcRecursiveGlobIndex = srcPattern.IndexOf("**", StringComparison.InvariantCulture);

        if (string.Equals(srcFile, srcPattern, StringComparison.InvariantCultureIgnoreCase) || srcRecursiveGlobIndex == -1 || !baseRecursive)
        {
            // No glob pattern in source pattern
            // No recursive glob pattern in source pattern

            return RanameWithPattern(Path.Combine(baseDir, Path.GetFileName(srcFile)), namePattern).Replace("\\", "/");
        }

        // find the first slash right before the glob index
        var srcLastSlash = srcPattern.LastIndexOf('/', srcRecursiveGlobIndex);

        // get the base path of the source pattern
        var srcBasePath = srcPattern.Substring(0, srcLastSlash + 1);
        
        // Compute the relative path from the base src directory
        var relativePath = MakeRelative(srcBasePath, srcFile);

        // Combine the destination base path with the relative path
        var outputPath = Path.Combine(baseDir, relativePath);
        
        return RanameWithPattern(outputPath, namePattern).Replace("\\", "/");
    }

    private static string RanameWithPattern(string fileName, string pattern)
    {
        if (string.IsNullOrEmpty(pattern) || pattern == "*" || pattern == "*.*")
        {
            // nohing to do
            return fileName;
        }

        // read the new extension pattern
        var newExt = Path.GetExtension(pattern);

        // read the new name pattern
        var newName = pattern.Substring(0, pattern.Length - newExt.Length);

        if (newName == "*")
        {
            // change then extension only
            fileName = Path.ChangeExtension(fileName, newExt);
        }
        else
        {
            newName = newName.Replace("*", Path.GetFileNameWithoutExtension(fileName));

            if (newExt == ".*")
            {
                // change the name only, so set the extension to the current extension
                newExt = Path.GetExtension(fileName);
            }

            fileName = Path.Combine(Path.GetDirectoryName(fileName)!, newName + newExt);
        }

        return fileName;
    }

    public static string AppendPathSeparatorChar(string path)
    {
        if (Path.HasExtension(path))
        {
            // it is a file, no need of the slash
            return path;
        }

        if (path[path.Length - 1] == Path.DirectorySeparatorChar)
        {
            // already ends with the slash
            return path;
        }

        return path + Path.DirectorySeparatorChar;
    }

    /// <summary>
    /// Replaces all path separators based on the current system settings.
    /// </summary>
    public static string NormalizePath(this string path)
    {
        // if is unix replace backslash with the slash
        if (Path.DirectorySeparatorChar == '/')
        {
            return path.Replace("\\", "/").Replace("/ ", "\\ ");
        }

        return path.Replace("/", "\\");
    }

    public static string AsPathSegment(this string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

        if (path[0] != Path.DirectorySeparatorChar)
        {
            // if it doesn't start with the slash, prepend it
            path = Path.DirectorySeparatorChar + path;
        }

        if (path[path.Length - 1] != Path.DirectorySeparatorChar)
        {
            // if it doesn't end with the slash, append it
            path += Path.DirectorySeparatorChar;
        }

        return path;
    }

}