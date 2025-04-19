using System;
using Microsoft.Extensions.Logging;
using Shr2.Models;
using Shr2.Interfaces;

namespace Shr2
{
    public class EnvConfig : IConfig
    {
        private static Config? config;
        private readonly ILogger<EnvConfig>? _logger;
        private readonly JConfig _jsonConfig;

        public EnvConfig(ILogger<EnvConfig>? logger = null, ILoggerFactory? loggerFactory = null)
        {
            _logger = logger;

            // Create a logger for JConfig if a logger factory is provided
            ILogger<JConfig>? jConfigLogger = null;
            if (loggerFactory != null)
            {
                jConfigLogger = loggerFactory.CreateLogger<JConfig>();
                _logger?.LogInformation("Created logger for JConfig using logger factory");
            }
            else
            {
                _logger?.LogWarning("No logger factory provided, JConfig will use null logger");
            }

            _jsonConfig = new JConfig(jConfigLogger);
            _logger?.LogInformation("EnvConfig initialized");
        }

        public Config GetConfig()
        {
            if (config != null)
                return config;

            try
            {
                // First try to load from JSON file
                _logger?.LogInformation("Loading configuration from JSON file");
                var jsonConfig = _jsonConfig.GetConfig();
                _logger?.LogInformation("JSON configuration loaded successfully");

                // Then override with environment variables if they exist
                _logger?.LogInformation("Applying environment variable overrides");
                config = new Config
                {
                    StorageConnectionString = GetEnvOrDefault("SHR2_STORAGE_CONNECTION_STRING", jsonConfig.StorageConnectionString),
                    StorageProvider = GetEnvOrDefault("SHR2_STORAGE_PROVIDER", jsonConfig.StorageProvider),
                    Domain = GetEnvOrDefault("SHR2_DOMAIN", jsonConfig.Domain),
                    EncodeWithPermissionKey = GetEnvBoolOrDefault("SHR2_ENCODE_WITH_PERMISSION_KEY", jsonConfig.EncodeWithPermissionKey),
                    PermissionKeys = GetEnvArrayOrDefault("SHR2_PERMISSION_KEYS", jsonConfig.PermissionKeys)
                };

                _logger?.LogInformation("Configuration loaded with environment variable overrides");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading configuration");
                throw;
            }
            return config;
        }

        public Config GetConfig(string path)
        {
            try
            {
                _logger?.LogInformation("Loading configuration from path: {Path}", path);

                // Load from specific JSON file first
                var jsonConfig = _jsonConfig.GetConfig(path);
                _logger?.LogInformation("JSON configuration loaded successfully from path: {Path}", path);

                // Then override with environment variables
                _logger?.LogInformation("Applying environment variable overrides");
                var config = new Config
                {
                    StorageConnectionString = GetEnvOrDefault("SHR2_STORAGE_CONNECTION_STRING", jsonConfig.StorageConnectionString),
                    StorageProvider = GetEnvOrDefault("SHR2_STORAGE_PROVIDER", jsonConfig.StorageProvider),
                    Domain = GetEnvOrDefault("SHR2_DOMAIN", jsonConfig.Domain),
                    EncodeWithPermissionKey = GetEnvBoolOrDefault("SHR2_ENCODE_WITH_PERMISSION_KEY", jsonConfig.EncodeWithPermissionKey),
                    PermissionKeys = GetEnvArrayOrDefault("SHR2_PERMISSION_KEYS", jsonConfig.PermissionKeys)
                };

                _logger?.LogInformation("Configuration loaded with environment variable overrides");
                return config;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading configuration from path: {Path}", path);
                throw;
            }
        }

        private string GetEnvOrDefault(string envName, string defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(envName);
            if (string.IsNullOrEmpty(value))
            {
                _logger?.LogDebug("Environment variable {EnvName} not found, using default value", envName);
                return defaultValue;
            }
            else
            {
                _logger?.LogInformation("Using environment variable {EnvName} with value: {Value}", envName, value);
                return value;
            }
        }

        private bool GetEnvBoolOrDefault(string envName, bool defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(envName);
            if (string.IsNullOrEmpty(value))
            {
                _logger?.LogDebug("Environment variable {EnvName} not found, using default value: {DefaultValue}", envName, defaultValue);
                return defaultValue;
            }
            else
            {
                try
                {
                    bool parsedValue = bool.Parse(value);
                    _logger?.LogInformation("Using environment variable {EnvName} with value: {Value}", envName, parsedValue);
                    return parsedValue;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to parse environment variable {EnvName} as boolean, using default value: {DefaultValue}", envName, defaultValue);
                    return defaultValue;
                }
            }
        }

        private string[] GetEnvArrayOrDefault(string envName, string[] defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(envName);
            if (string.IsNullOrEmpty(value))
            {
                _logger?.LogDebug("Environment variable {EnvName} not found, using default value with {Count} items", envName, defaultValue.Length);
                return defaultValue;
            }
            else
            {
                var parsedValue = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                _logger?.LogInformation("Using environment variable {EnvName} with {Count} items", envName, parsedValue.Length);
                return parsedValue;
            }
        }
    }
}
