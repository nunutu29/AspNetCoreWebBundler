using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AspNetCoreWebBundler
{
    /// <summary>
    /// Should only be used in DEBUG mode.
    /// This allows to consume bundles at runtime.
    /// </summary>
    internal class RuntimeWebBundlerService(IHostApplicationLifetime hostApplicationLifetime, ILogger<RuntimeWebBundlerService> logger) : IHostedService, IDisposable
    {
        private CancellationTokenRegistration? _startRegistration;
        private CancellationTokenRegistration? _stoppingRegistration;

        private RuntimeWebBundler _watcher;

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _startRegistration = hostApplicationLifetime.ApplicationStarted.Register(StartMinifier);
            _stoppingRegistration = hostApplicationLifetime.ApplicationStopping.Register(StopMinifier);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        private void StartMinifier()
        {
            var projects = RuntimeWebBundler.EnumerateProjects().ToList();

            if (projects.Count == 0)
            {
                return;
            }

            _watcher = new RuntimeWebBundler(logger, projects);

            logger.LogInformation("Starting RuntimeBundler...");

            if (_watcher.Start())
            {
                logger.LogInformation("RuntimeBundler started successfully");
            }
            else
            {
                logger.LogWarning("Could not start RuntimeBundler: No output file names were matched");
            }
        }

        private void StopMinifier()
        {
            logger.LogInformation("Stopping RuntimeBundler");
            _watcher?.Stop();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _startRegistration?.Dispose();
            _stoppingRegistration?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}