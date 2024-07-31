using System;

namespace AspNetCoreWebBundler
{
    /// <summary>
    /// Attribute used to mark an assembly with information about a project for runtime bundling.
    /// This attribute allows specifying the project directory and project name, which can be used
    /// for tasks such as runtime minification and bundling during the development process.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    public sealed class RuntimeWebBundlerProjectAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeWebBundlerProjectAttribute"/> class.
        /// </summary>
        /// <param name="projectName">The name of the project (including the extension).</param>
        /// <param name="projectDirectory">The directory where the project file resides.</param>

        public RuntimeWebBundlerProjectAttribute(string projectName, string projectDirectory)
        {
            ProjectName = projectName;
            ProjectDirectory = projectDirectory;
        }

        /// <summary>
        /// Gets the name of the project (including the extension).
        /// </summary>
        public string ProjectName { get; }

        /// <summary>
        /// Gets the directory where the project file resides.
        /// </summary>
        public string ProjectDirectory { get; }
    }
}