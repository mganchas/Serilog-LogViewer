using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using Microsoft.Extensions.Logging;

namespace LogViewer.Services
{
    public static class LoggerService
    {
        private static Microsoft.Extensions.Logging.ILogger logger;
        public static Microsoft.Extensions.Logging.ILogger Logger {
            get
            {
                if (logger == null)
                {
                    SetupLogging();
                }
                return logger;
            }
        } 

        private static IEnumerable<string> GetConfigFiles(string path)
        {
            return from f in GetConfigFiles(path, "*.config.json")
                   orderby f.Substring(0, f.LastIndexOf('.'))
                   select f;
        }

        private static IEnumerable<string> GetConfigFiles(string path, string searchPattern)
        {
            foreach (string item in Directory.EnumerateFiles(path, searchPattern))
            {
                yield return item;
            }
            foreach (string item2 in Directory.EnumerateDirectories(path))
            {
                foreach (string configFile in GetConfigFiles(item2, searchPattern))
                {
                    yield return configFile;
                }
            }
        }

        public static IConfiguration LoadConfiguration(bool expandEnvironmentVariables = false)
        {
            string directoryName = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(ConfigurationManager.AppSettings.AllKeys.ToDictionary((string k) => k, (string k) => ConfigurationManager.AppSettings[k]));

            foreach (string configFile in GetConfigFiles(directoryName))
            {
                configurationBuilder.AddJsonFile(configFile);
            }

            IConfigurationRoot configurationRoot = configurationBuilder.Build();
            if (expandEnvironmentVariables)
            {
                {
                    foreach (KeyValuePair<string, string> item in configurationRoot.AsEnumerable())
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            ((IConfiguration)configurationRoot)[item.Key] = Environment.ExpandEnvironmentVariables(item.Value);
                        }
                    }
                    return configurationRoot;
                }
            }
            return configurationRoot;
        }

        public static ILoggerFactory SetupLogging()
        {
            LoggerFactory loggerFactory = new LoggerFactory();
            var configuration = LoadConfiguration(expandEnvironmentVariables: true);

            LoggerConfiguration loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration, null);
            Logger logger = loggerConfiguration.CreateLogger();
            loggerFactory.AddSerilog(logger, true);

            LoggerService.logger = loggerFactory.CreateLogger("LogViewer");

            return loggerFactory;
        }
    }

    public static class JsonConfigurationExtensions
    {
        public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder builder, string path)
        {
            return builder.AddJsonFile(path);
        }
    }
}
