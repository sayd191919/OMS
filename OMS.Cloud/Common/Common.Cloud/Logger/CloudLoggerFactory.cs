﻿using OMS.Common.Cloud.Names;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace OMS.Common.Cloud.Logger
{
    public static class CloudLoggerFactory
    {
        private const string loggerSourceNameKey = "loggerSourceNameKey";
        
        private static Dictionary<string, CloudLogger> loggers;
        private static Dictionary<string, CloudLogger> Loggers
        {
            get
            {
                return loggers ?? (loggers = new Dictionary<string, CloudLogger>());
            }
        }

        public static ICloudLogger GetLogger(string sourceName = null)
        {
            sourceName = ResolveSourceName(sourceName);

            if (!Loggers.ContainsKey(sourceName))
            {
                Loggers[sourceName] = new CloudLogger(sourceName);
            }

            return Loggers[sourceName];
        }

        private static string ResolveSourceName(string sourceName)
        {
            if (sourceName == null)
            {
                if (ConfigurationManager.AppSettings[loggerSourceNameKey] is string loggerSourceNameValue)
                {
                    sourceName = loggerSourceNameValue;
                }
                else
                {
                    throw new KeyNotFoundException($"Key '{loggerSourceNameKey}' not found in appSettings.");
                }
            }

            HashSet<string> loggerSourceNames = typeof(LoggerSourceNames).GetFields(BindingFlags.Public | BindingFlags.Static)
                                                                         .Where(f => f.FieldType == typeof(string))
                                                                         .Select(f => (string)f.GetValue(null))
                                                                         .ToHashSet<string>();
            if (!loggerSourceNames.Contains(sourceName))
            {
                sourceName = "Unknown";
            }

            return sourceName;
        }
    }
}