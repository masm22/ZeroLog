﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using ZeroLog.Appenders.Builders;
using ZeroLog.Config;
using ZeroLog.Utils;

namespace ZeroLog.ConfigResolvers
{
    public static class Configurator
    {
        public static ILogManager ConfigureAndWatch(string configFilePath)
        {
            var configFileFullPath = Path.GetFullPath(configFilePath);

            var resolver = new HierarchicalResolver();

            var config = ConfigureResolver(configFileFullPath, resolver);

            var watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(configFileFullPath),
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            watcher.Changed += (sender, args) =>
            {
                try
                {
                    if (!string.Equals(args.FullPath, configFileFullPath, StringComparison.InvariantCultureIgnoreCase))
                        return;

                    var newConfig = ReadConfiguration(configFileFullPath);
                    resolver.Build(newConfig);
                }
                catch (Exception e)
                {
                    LogManager.GetLogger(typeof(Configurator))
                        .FatalFormat("Updating config failed with: {0}", e.Message);
                }
            };

            var lecagyConfiguration = CreateLegacyConfiguration(config);

            return LogManager.Initialize(resolver, lecagyConfiguration);
        }

        private static ZeroLogConfiguration ConfigureResolver(string configFileFullPath, HierarchicalResolver resolver)
        {
            var config = ReadConfiguration(configFileFullPath);
            resolver.Build(config);
            return config;
        }

        private static ZeroLogConfiguration ReadConfiguration(string configFilePath)
        {
            var filecontent = ReadFileContentWithRetry(configFilePath);
            return DeserializeConfiguration(filecontent);
        }

        private static LogManagerConfiguration CreateLegacyConfiguration(ZeroLogConfiguration config)
        {
            var legacyConfig = new LogManagerConfiguration
            {
                Level = config.RootLogger.Level,
                LogEventPoolExhaustionStrategy = config.RootLogger.LogEventPoolExhaustionStrategy,
                LogEventBufferSize = config.LogEventBufferSize,
                LogEventQueueSize = config.LogEventQueueSize
            };

            return legacyConfig;
        }

        public static ZeroLogConfiguration DeserializeConfiguration(string jsonConfiguration)
        {
            var config = JsonExtensions.DeserializeOrDefault(jsonConfiguration, new ZeroLogConfiguration());
            return config;
        }

        private static string ReadFileContentWithRetry(string filepath)
        {
            const int numberOfRetries = 3;
            const int delayOnRetry = 1000;

            for (var i = 0; i < numberOfRetries; i++)
            {
                try
                {
                    return File.ReadAllText(filepath);
                }
                catch (IOException)
                {
                    Thread.Sleep(delayOnRetry);
                }
            }

            return null;
        }
    }
}