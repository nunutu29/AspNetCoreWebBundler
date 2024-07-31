using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using NUglify;
using NUglify.JavaScript;

namespace AspNetCoreWebBundler
{
    internal class BundleUglifier
    {
        public event EventHandler<BundleUglifierEventArgs> BeforeWritingMinFile;
        public event EventHandler<BundleUglifierEventArgs> AfterWritingMinFile;
        public event EventHandler<BundleUglifierEventArgs> BeforeWritingGzipFile;
        public event EventHandler<BundleUglifierEventArgs> AfterWritingGzipFile;
        public event EventHandler<BundleUglifierEventArgs> BeforeWritingSourceMap;
        public event EventHandler<BundleUglifierEventArgs> AfterWritingSourceMap;
        public event EventHandler<BundleUglifierEventArgs> ErrorMinifyingFile;

        protected BundleUglifier()
        {
        }

        protected BundleUglifierResult MinifyBundle(Bundle bundle)
        {
            var minResult = new BundleUglifierResult(bundle.AbsoluteOutputFile);

            if (!string.IsNullOrEmpty(bundle.Content) && bundle.IsMinifyEnabled)
            {
                var extension = Path.GetExtension(bundle.AbsoluteOutputFile).ToUpperInvariant();

                try
                {
                    switch (extension)
                    {
                        case ".JS":
                            MinifyJavaScript(bundle, minResult);
                            break;
                        case ".CSS":
                            MinifyCss(bundle, minResult);
                            break;
                        case ".HTML":
                        case ".HTM":
                            MinifyHtml(bundle, minResult);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    AddGenericException(minResult, ex);
                }
            }

            if (minResult.HasErrors)
            {
                OnErrorMinifyingFile(minResult);
            }

            return minResult;
        }

        protected void GZipFile(string sourceFile, Bundle bundle, bool changed, string content)
        {
            var gzipFile = sourceFile + ".gz";
            var containsChanges = changed || File.GetLastWriteTimeUtc(gzipFile) < File.GetLastWriteTimeUtc(sourceFile);

            if (containsChanges)
            {
                OnBeforeWritingGzipFile(sourceFile, gzipFile, bundle, true);
                
                var contentBytes = Encoding.UTF8.GetBytes(content);

                using (var fileStream = File.OpenWrite(gzipFile))
                {
                    using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal))
                    {
                        gzipStream.Write(contentBytes, 0, contentBytes.Length);
                    }
                }

                OnAfterWritingGzipFile(sourceFile, gzipFile, bundle, true);
            }
        }

        private void MinifyJavaScript(Bundle bundle, BundleUglifierResult minResult)
        {
            // File name to use in error reporting.
            var fileNameReport = minResult.FileName;

            if (bundle.AbsoluteInputFiles.Count == 1)
            {
                fileNameReport = bundle.AbsoluteInputFiles[0];
            }

            var settings = bundle.Minify.JavaScriptSettings();

            if (!bundle.SourceMap)
            {
                var uglifyResult = Uglify.Js(bundle.Content, fileNameReport, settings);
                WriteMinFile(bundle, minResult, uglifyResult);
            }
            else
            {
                string minFile = FileHelper.GetMinFileName(minResult.FileName);
                string mapFile = minFile + ".map";

                using (StringWriter writer = new StringWriter())
                {
                    using (V3SourceMap sourceMap = new V3SourceMap(writer))
                    {
                        settings.SymbolsMap = sourceMap;
                        sourceMap.StartPackage(minFile, mapFile);
                        sourceMap.SourceRoot = bundle.SourceMapRootPath;

                        var uglifyResult = Uglify.Js(bundle.Content, fileNameReport, settings);
                        WriteMinFile(bundle, minResult, uglifyResult);
                    }

                    minResult.SourceMapChanged |= WriteSourceMapFile(bundle, minFile, mapFile, writer.ToString());
                }
            }
        }

        private void MinifyCss(Bundle bundle, BundleUglifierResult minResult)
        {
            // File name to use in error reporting.
            var fileNameReport = minResult.FileName;

            if (bundle.AbsoluteInputFiles.Count == 1)
            {
                fileNameReport = bundle.AbsoluteInputFiles[0];
            }

            var settings = bundle.Minify.CssSettings();
            var uglifyResult = Uglify.Css(bundle.Content, fileNameReport, settings);
            WriteMinFile(bundle, minResult, uglifyResult);
        }

        private void MinifyHtml(Bundle bundle, BundleUglifierResult minResult)
        {
            // File name to use in error reporting.
            var fileNameReport = minResult.FileName;

            if (bundle.AbsoluteInputFiles.Count == 1)
            {
                fileNameReport = bundle.AbsoluteInputFiles[0];
            }

            var settings = bundle.Minify.HtmlSettings();
            var uglifyResult = Uglify.Html(bundle.Content, settings, fileNameReport);
            WriteMinFile(bundle, minResult, uglifyResult);
        }

        private void WriteMinFile(Bundle bundle, BundleUglifierResult minResult, UglifyResult uglifyResult)
        {
            minResult.MinifiedContent = uglifyResult.Code?.Trim();

            if (!uglifyResult.HasErrors)
            {
                var minFile = FileHelper.GetMinFileName(minResult.FileName);
                var containsChanges = FileHelper.HasContentChanged(minFile, minResult.MinifiedContent);
                minResult.Changed |= containsChanges;

                if (containsChanges)
                {
                    OnBeforeWritingMinFile(minResult.FileName, minFile, bundle, true);
                    
                    FileHelper.CreateParentDirectory(minFile);
                    FileHelper.Write(minFile, minResult.MinifiedContent);

                    OnAfterWritingMinFile(minResult.FileName, minFile, bundle, true);
                }
            }
            else
            {
                AddNUglifyErrors(uglifyResult, minResult);
            }
        }

        private bool WriteSourceMapFile(Bundle bundle, string minFile, string smFile, string smContent)
        {
            var smChanged = FileHelper.HasContentChanged(smFile, smContent);

            if (smChanged)
            {
                // invoke event before
                OnBeforeWritingSourceMap(minFile, smFile, bundle, true);

                // write source map file
                FileHelper.Write(smFile, smContent);

                // invoke event after
                OnAfterWritingSourceMap(minFile, smFile, bundle, true);

                return true;
            }

            return false;
        }

        private static void AddNUglifyErrors(UglifyResult minifier, BundleUglifierResult minResult)
        {
            foreach (var error in minifier.Errors)
            {
                var minError = new BundleUglifierError
                {
                    FileName = minResult.FileName,
                    Message = error.Message,
                    LineNumber = error.StartLine,
                    ColumnNumber = error.StartColumn
                };

                minResult.Errors.Add(minError);
            }
        }

        private static void AddGenericException(BundleUglifierResult minResult, Exception ex)
        {
            minResult.Errors.Add(new BundleUglifierError
            {
                FileName = minResult.FileName,
                Message = ex.Message,
                LineNumber = 0,
                ColumnNumber = 0
            });
        }

        protected void OnBeforeWritingMinFile(string file, string minFile, Bundle bundle, bool containsChanges)
        {
            BeforeWritingMinFile?.Invoke(this, new BundleUglifierEventArgs(file, minFile, bundle, containsChanges));
        }

        protected void OnAfterWritingMinFile(string file, string minFile, Bundle bundle, bool containsChanges)
        {
            AfterWritingMinFile?.Invoke(this, new BundleUglifierEventArgs(file, minFile, bundle, containsChanges));
        }

        protected void OnBeforeWritingGzipFile(string minFile, string gzipFile, Bundle bundle, bool containsChanges)
        {
            BeforeWritingGzipFile?.Invoke(this, new BundleUglifierEventArgs(minFile, gzipFile, bundle, containsChanges));
        }

        protected void OnAfterWritingGzipFile(string minFile, string gzipFile, Bundle bundle, bool containsChanges)
        {
            AfterWritingGzipFile?.Invoke(this, new BundleUglifierEventArgs(minFile, gzipFile, bundle, containsChanges));
        }

        protected void OnErrorMinifyingFile(BundleUglifierResult result)
        {
            ErrorMinifyingFile?.Invoke(this, new BundleUglifierEventArgs(result.FileName, null, null, false)
            {
                Result = result
            });
        }

        protected void OnBeforeWritingSourceMap(string file, string mapFile, Bundle bundle, bool containsChanges)
        {
            BeforeWritingSourceMap?.Invoke(this, new BundleUglifierEventArgs(file, mapFile, containsChanges));
        }

        protected void OnAfterWritingSourceMap(string file, string mapFile, Bundle bundle, bool containsChanges)
        {
            AfterWritingSourceMap?.Invoke(this, new BundleUglifierEventArgs(file, mapFile, containsChanges));
        }

    }
}
