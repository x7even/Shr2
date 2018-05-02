using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shr2.Interfaces;

namespace Shr2.Services
{
    public class Initializer : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;


        public Initializer(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;


        public Task StartAsync(CancellationToken cancellationToken)
        {
            var storage = _serviceProvider.GetService<IStorageProvider>();
            if (storage != null)
            {
                //Init anything required to init.
                var spInit = storage.Init().Result;
            }


            return Task.CompletedTask;
        }

        public async Task InitServices(IStorageProvider storageProvider)
        {
            var initsp = await storageProvider.Init();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            //dispose any managed resources.
        }
    }
}
