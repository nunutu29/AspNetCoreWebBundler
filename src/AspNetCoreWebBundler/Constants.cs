using System.Reflection;

namespace AspNetCoreWebBundler
{
    internal class Constants
    {
        private static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();

        public static readonly string PackageName = CurrentAssembly.GetName().Name;
        public static readonly string PackageVersion = CurrentAssembly.GetName().Version.ToString();

        public const string ConfigFileName = "AspNetCoreWebBundler.json";
    }
}