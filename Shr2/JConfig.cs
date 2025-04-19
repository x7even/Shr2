﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;
using Shr2.Models;
using Shr2.Interfaces;

namespace Shr2
{
    public class JConfig : IConfig
    {
        private static Config? config;
        private readonly ILogger<JConfig>? _logger;

        public JConfig(ILogger<JConfig>? logger = null)
        {
            _logger = logger;
        }

        public Config GetConfig()
        {
            if (config != null)
                return config;

            // Check for environment variable with config path
            string configPath = Environment.GetEnvironmentVariable("SHR2_CONFIG_PATH") ?? "shr2.config.json";
            _logger?.LogInformation("Loading configuration from {ConfigPath}", configPath);

            return GetConfig(configPath);
        }

        public Config GetConfig(string path)
        {
            try
            {
                var jresult = new JsonFileReader<Config>().ReadJsonFile(path);
                if (jresult == null)
                {
                    _logger?.LogError("Unable to read expected json config model in [{Path}]", path);
                    throw new FormatException($"Unable to read expected json config model in [{path}]");
                }

                return config = jresult;
            }
            catch (FileNotFoundException ex)
            {
                _logger?.LogError(ex, "Configuration file not found: {Path}", path);
                throw new FileNotFoundException($"Configuration file not found: {path}", ex);
            }
            catch (Exception ex) when (ex is not FormatException)
            {
                _logger?.LogError(ex, "Error reading configuration file: {Path}", path);
                throw;
            }
        }
    }
}
