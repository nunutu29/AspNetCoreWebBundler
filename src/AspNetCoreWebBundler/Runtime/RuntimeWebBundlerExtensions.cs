using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreWebBundler
{
    /// <summary>
    /// Provides extension methods for adding runtime minification services.
    /// </summary>
    public static class RuntimeWebBundlerExtensions
    {
        /// <summary>
        /// Registers the RuntimeBundlerService background service, which monitors source files and triggers automatic re-bundling.
        /// This method should only be used in DEBUG mode.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the RuntimeMinifier to.</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        public static IServiceCollection AddRuntimeWebBundler(this IServiceCollection services)
        {
            services.AddHostedService<RuntimeWebBundlerService>();
            return services;
        }
    }
}