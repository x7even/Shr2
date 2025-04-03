﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shr2.Interfaces;

namespace Shr2.Services
{
    public class Initializer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Initializer>? _logger;

        public Initializer(
            IServiceProvider serviceProvider,
            ILogger<Initializer>? logger = null)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger?.LogInformation("Initializer service starting");
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var storage = scope.ServiceProvider.GetRequiredService<IStorageProvider>();
                
                _logger?.LogInformation("Initializing storage provider");
                var initialized = await storage.Init();
                
                if (initialized)
                {
                    _logger?.LogInformation("Storage provider initialized successfully");
                }
                else
                {
                    _logger?.LogWarning("Storage provider initialization returned false");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing services");
                throw;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger?.LogInformation("Initializer service stopping");
            return base.StopAsync(cancellationToken);
        }
    }
}
