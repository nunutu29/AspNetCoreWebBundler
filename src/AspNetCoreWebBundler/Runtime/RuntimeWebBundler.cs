using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace AspNetCoreWebBundler
{
    internal class RuntimeWebBundler
    {
        private static readonly string[] IgnorePathSegmnets =
        {
            "node_modules".AsPathSegment(),
            "bower_components".AsPathSegment(),
            "jspm_packages".AsPathSegment()
        };

        private readonly ILogger _logger;
        private readonly HashSet<string> _configPaths;
        private readonly BundleProcessor _processor;
        private readonly ConcurrentDictionary<string, FileSystemWatcher> _listeners;
        private readonly ConcurrentDictionary<string, QueueItem> _queue;
        private readonly Timer _timer;

        public RuntimeWebBundler(ILogger logger) : this(logger, EnumerateProjects())
        {

        }

        public RuntimeWebBundler(ILogger logger, IEnumerable<string> projects)
        {
            _logger = logger;
            _configPaths = new HashSet<string>(projects.Select(path => Path.Combine(path, Constants.ConfigFileName)), StringComparer.InvariantCultureIgnoreCase);

            _processor = new BundleProcessor();
            _processor.Processing += (s, e) =>
            {
                _logger.LogDebug("Processing " + e.Bundle.AbsoluteOutputFile);
                FileHelper.RemoveReadonly(e.Bundle.AbsoluteOutputFile);
            };
            _processor.AfterBundling += (s, e) =>
            {
                _logger.LogDebug("Bundled " + e.Bundle.AbsoluteOutputFile);
            };
            _processor.BeforeWritingMinFile += (s, e) =>
            {
                FileHelper.RemoveReadonly(e.ResultFile);
            };
            _processor.AfterWritingMinFile += (s, e) =>
            {
                _logger.LogDebug("Minified " + e.ResultFile);
            };
            _processor.ErrorMinifyingFile += (s, e) =>
            {
                if (e.Result == null || !e.Result.HasErrors)
                {
                    return;
                }

                foreach (var error in e.Result.Errors)
                {
                    _logger.LogError(error.ToString());
                }
            };
            _processor.BeforeWritingGzipFile += (s, e) =>
            {
                FileHelper.RemoveReadonly(e.ResultFile);
            };
            _processor.AfterWritingGzipFile += (s, e) =>
            {
                _logger.LogDebug("GZipped " + e.ResultFile);
            };
            _processor.BeforeWritingSourceMap += (s, e) =>
            {
                FileHelper.RemoveReadonly(e.ResultFile);
            };
            _processor.AfterWritingSourceMap += (s, e) =>
            {
                _logger.LogDebug("SourceMap " + e.ResultFile);
            };
            _processor.MinificationSkipped += (s, e) =>
            {
                _logger.LogDebug("No changes, skipping minification of " + e.Bundle.AbsoluteOutputFile);
            };
            
            _listeners = new ConcurrentDictionary<string, FileSystemWatcher>();
            _queue = new ConcurrentDictionary<string, QueueItem>();
            _timer = new Timer(TimerElapsed, null, 0, Timeout.Infinite);
        }

        public bool Start()
        {
            var started = false;

            foreach (var configPath in _configPaths)
            {
                _logger.LogDebug("Check config: {configPath}", configPath);

                try
                {
                    if (File.Exists(configPath))
                    {
                        var projectRoot = new FileInfo(configPath).DirectoryName;

                        var fsw = new FileSystemWatcher(projectRoot);

                        fsw.Changed += FileChanged;
                        fsw.Renamed += FileChanged;

                        fsw.IncludeSubdirectories = true;
                        fsw.NotifyFilter = NotifyFilters.Size | NotifyFilters.CreationTime | NotifyFilters.FileName;
                        fsw.EnableRaisingEvents = true;
                        
                        _listeners.TryAdd(projectRoot, fsw);

                        _logger.LogDebug("Watching: {projectRoot}", projectRoot);

                        started = true;
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }

            // re-process just before the projects starts (one might change the source files at build time, but after the task bundle)
            foreach (var configPath in _configPaths)
            {
                _queue.TryAdd(configPath, new QueueItem(configPath));
            }

            return started;
        }

        public void Stop()
        {
            try
            {
                foreach (var fsw in _listeners.Values)
                {
                    fsw.EnableRaisingEvents = false;

                    fsw.Changed -= FileChanged;
                    fsw.Changed -= FileChanged;

                    try
                    {
                        fsw.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }

                _listeners.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            FileChangedCore((FileSystemWatcher)sender, e.FullPath);
        }

        private void FileChangedCore(FileSystemWatcher fsw, string fullPath)
        {
            if (_configPaths.Contains(fullPath))
            {
                fsw.EnableRaisingEvents = false;
                _queue.TryAdd(fullPath, new QueueItem(fullPath));
            }
            else if (IsFileValid(fullPath))
            {
                fsw.EnableRaisingEvents = false;

                var root = _listeners.Keys.FirstOrDefault(path => fullPath.StartsWith(path, StringComparison.InvariantCultureIgnoreCase));

                if (root != null)
                {
                    var configFile = _configPaths.FirstOrDefault(path => path.StartsWith(root, StringComparison.InvariantCultureIgnoreCase));
                    _queue.TryAdd(fullPath, new QueueItem(configFile));
                }
            }
        }

        public static bool IsFileValid(string file)
        {
            string fileName = Path.GetFileName(file);

            // VS adds ~ to temp file names so let's ignore those
            if (fileName.Contains('~') || fileName.Contains(".min."))
            {
                return false;
            }

            if (IgnorePathSegmnets.Any(p => file.IndexOf(p, StringComparison.InvariantCultureIgnoreCase) > -1))
            {
                return false;
            }

            if (!BundleProcessor.IsSupported(file))
            {
                return false;
            }

            return true;
        }

        private void TimerElapsed(object state)
        {
            try
            {
                var items = _queue.Where(i => i.Value.Timestamp < DateTime.Now.AddMilliseconds(-250)).ToList();
                
                foreach (var item in items)
                {
                    if (item.Value.ConfigFile == item.Key)
                    {
                        // process when configuration file changed
                        _processor.Process(item.Value.ConfigFile);
                    }
                    else
                    { 
                        // process by source file
                        _processor.ProcessBySourceFile(item.Value.ConfigFile, item.Key);
                    }
                    
                    // remove from the queue
                    _queue.TryRemove(item.Key, out _);
                }

                foreach (var fsw in _listeners.Values)
                {
                    fsw.EnableRaisingEvents = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                // Only now fire off next event to avoid overlapping timers
                _timer.Change(250, Timeout.Infinite);
            }
        }

        public static IEnumerable<string> EnumerateProjects()
        {
            var element = Assembly.GetEntryAssembly();

            if (element != null)
            {
                var attrs = element.GetCustomAttributes<RuntimeWebBundlerProjectAttribute>();

                foreach (var attr in attrs)
                {
                    yield return attr.ProjectDirectory;
                }
            }
        }

        private class QueueItem
        {
            public QueueItem(string configFile)
            {
                ConfigFile = configFile;
                Timestamp = DateTime.Now;
            }

            public DateTime Timestamp { get; }
            public string ConfigFile { get; }
        }
    }
}