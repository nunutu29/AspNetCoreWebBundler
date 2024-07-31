using System.Collections.Generic;

namespace AspNetCoreWebBundler
{
    internal class BundleUglifierResult
    {
        public BundleUglifierResult(string fileName)
        {
            FileName = fileName;
        }

        /// <summary>
        /// Gets the output absolute file name.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the minified content.
        /// </summary>
        public string MinifiedContent { get; set; }

        /// <summary>
        /// A collection of any errors reported by the compiler.
        /// </summary>
        public List<BundleUglifierError> Errors { get; } = new List<BundleUglifierError>();

        /// <summary>
        /// Checks if the compilation resulted in errors.
        /// </summary>
        public bool HasErrors => Errors.Count > 0;

        /// <summary>
        /// Checks if the minified content has changed.
        /// </summary>
        public bool Changed { get; set; }

        /// <summary>
        /// Checks if the source map content has changed.
        /// </summary>
        public bool SourceMapChanged { get; set; }
    }
}
