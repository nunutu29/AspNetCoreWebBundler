using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using static System.Net.WebRequestMethods;

namespace AspNetCoreWebBundler
{
    internal class Bundle
    {
        private List<string> _absoluteFiles;
        private HashSet<string> _files;
        private Dictionary<string, string> _filesPattern;

        private string _absoluteOutputFile;

        /// <summary>
        /// Contains the current configuration file.
        /// </summary>
        internal string ConfigFile { get; set; }
        
        /// <summary>
        /// [Required] The output file or directory name.
        /// </summary>
        public string Dest { get; set; }

        /// <summary>
        /// [Required] Source files. At least one file is required.
        ///  </summary>
        /// <remarks>
        /// Globbing patterns are supported.
        /// </remarks>
        public List<string> Src { get; } = new();

        /// <summary>
        /// [Optional] Minify/Uglify settings.
        /// </summary>
        public BundleUglifierSettings Minify { get; } = new() { { "enabled", true } };

        /// <summary>
        /// [Optional] Enable javascript source map.
        /// </summary>
        public bool SourceMap { get; set; }

        /// <summary>
        /// [Optional] Sets an optional source root URI that will be added to the map object as the sourceRoot property if set
        /// </summary>
        public string SourceMapRootPath { get; set; }

        /// <summary>
        /// The content of all <see cref="Src"/>.
        /// </summary>
        internal string Content { get; set; }

        internal bool IsMinifyEnabled => Minify.GetValue("enabled", false);

        internal bool IsGZipEnabled => Minify.GetValue("gZip", false);
        
        internal DateTime MostRecentWrite { get; set; }
        
        /// <summary>
        /// Gets the absolute path of the <see cref="Dest"/>.
        /// </summary>
        internal string AbsoluteOutputFile
        {
            get
            {
                if (_absoluteOutputFile == null)
                {
                    _absoluteOutputFile = Path.Combine(GetConfigDirectory(), Dest.NormalizePath());
                }

                return _absoluteOutputFile;
            }
        }

        /// <summary>
        /// Retrieves the absolute file paths of all matching <see cref="Src"/>.
        /// The results are cached to optimize performance, so any subsequent calls return the same cached result.
        /// </summary>
        internal List<string> AbsoluteInputFiles
        {
            get
            {
                if (_absoluteFiles == null)
                {
                    ComputeSrc();

                    var folder = GetConfigDirectory();
                    _absoluteFiles = _files.Select(file => Path.Combine(folder, file.NormalizePath())).ToList();
                }

                return _absoluteFiles;
            }
        }

        private string GetConfigDirectory()
        {
            return new FileInfo(ConfigFile).DirectoryName;
        }

        private void ComputeSrc()
        {
            if (_files != null)
            {
                return;
            }

            _files = new HashSet<string>();
            _filesPattern = new Dictionary<string, string>();

            if (!Src.Any())
            {
                return;
            }
            
            var folder = GetConfigDirectory();
            var options = new Options { AllowWindowsPaths = true };

            string output, outputMin;

            if (PathHelper.IsDirectory(Dest) || Dest.IndexOf('*') > -1)
            {
                output = null;
                outputMin = null;
            }
            else
            {
                output = Dest;
                outputMin = FileHelper.GetMinFileName(Dest);
            }

            foreach (var inputFile in Src.Where(f => !f.StartsWith("!", StringComparison.Ordinal)))
            {
                var globIndex = inputFile.IndexOf('*');

                if (globIndex > -1)
                {
                    var relative = string.Empty;

                    // The search starts at a specified character position and proceeds backward toward the beginning of the string
                    var last = inputFile.LastIndexOf('/', globIndex);

                    if (last > -1)
                    {
                        relative = inputFile.Substring(0, last + 1);
                    }

                    var searchDir = Path.Combine(folder, relative).NormalizePath();

                    if (Directory.Exists(searchDir))
                    {
                        var ext = Path.GetExtension(inputFile);

                        var allFiles = Directory.EnumerateFiles(searchDir, "*" + ext, SearchOption.AllDirectories)
                            .Select(file => file
                                .Replace(folder + Path.DirectorySeparatorChar, "")
                                .Replace("\\", "/")
                            );

                        var files = Minimatcher.Filter(allFiles, inputFile, options)
                            .OrderBy(file => file);

                        foreach (var file in files)
                        {
                            if (file == output || file == outputMin)
                            {
                                continue;
                            }

                            if (_files.Add(file))
                            {
                                _filesPattern[file] = inputFile;
                            }
                        }
                    }

                }
                else
                {
                    var fullPath = Path.Combine(folder, inputFile);

                    if (PathHelper.IsDirectory(fullPath))
                    {
                        var ext = Path.GetExtension(Dest);

                        var files = Directory.EnumerateFiles(fullPath, "*" + ext, SearchOption.TopDirectoryOnly)
                            .Select(file => file
                                .Replace(folder + Path.DirectorySeparatorChar, "")
                                .Replace("\\", "/")
                        );

                        foreach (var file in files)
                        {
                            if (_files.Add(file))
                            {
                                _filesPattern[file] = inputFile;
                            }
                        }
                    }
                    else
                    {
                        if (_files.Add(inputFile))
                        {
                            _filesPattern[inputFile] = inputFile;
                        }
                    }
                }
            }
            
            // Remove files starting with a !
            foreach (var inputFile in Src.Where(f => f.StartsWith("!", StringComparison.Ordinal)))
            {
                var allFiles = Minimatcher.Filter(_files, inputFile, options);
                _files = new HashSet<string>(allFiles);
            }
        }

        internal IEnumerable<Bundle> Expand()
        {
            var globIndex = Dest.IndexOf('*');

            if (PathHelper.IsDirectory(Dest) || globIndex > -1)
            {
                ComputeSrc();

                if (globIndex == -1)
                {
                    // simple case: for each input file create an output file as there is no globbing pattern

                    foreach (var file in _files)
                    {
                        var bundle = new Bundle
                        {
                            ConfigFile = ConfigFile,
                            Dest = Path.Combine(Dest, Path.GetFileName(file)).Replace("\\", "/"),
                            SourceMap = SourceMap,
                            SourceMapRootPath = SourceMapRootPath
                        };

                        bundle.Src.Add(file);

                        foreach (var pair in Minify)
                        {
                            bundle.Minify[pair.Key] = pair.Value;
                        }

                        yield return bundle;
                    }
                }
                else
                {
                    // complex glob case
                    var destLastSlash = Dest.LastIndexOf('/', globIndex);
                    var recursive = Dest.IndexOf("**", StringComparison.InvariantCulture) > -1;

                    var baseDir = destLastSlash > -1 ? Dest.Substring(0, destLastSlash + 1) : Dest;
                    var namePattern = Path.GetFileName(Dest);

                    var files = _files.Select(file =>
                    {
                        var output = PathHelper.ComputeBundleOuputPath(baseDir, namePattern, recursive, file, _filesPattern[file]);

                        return new
                        {
                            Src = file,
                            Dest = output
                        };
                    });

                    var groups = files.GroupBy(file => file.Dest);

                    foreach (var group in groups)
                    {
                        var bundle = new Bundle
                        {
                            ConfigFile = ConfigFile,
                            Dest = group.Key,
                            SourceMap = SourceMap,
                            SourceMapRootPath = SourceMapRootPath
                        };

                        foreach (var pair in Minify)
                        {
                            bundle.Minify[pair.Key] = pair.Value;
                        }

                        foreach (var item in group)
                        {
                            bundle.Src.Add(item.Src);
                        }

                        yield return bundle;
                    }
                }

                yield break;
            }
            
            // file output bundle
            yield return this;
        }
    }
}
