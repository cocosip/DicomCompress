using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Volo.Abp;

namespace DicomCompress
{
    public class DicomCompressHostedService : IHostedService
    {
        private readonly IAbpApplicationWithExternalServiceProvider _application;
        private readonly IServiceProvider _serviceProvider;
        private readonly DicomCompressService _dicomCompressService;

        public DicomCompressHostedService(
            IAbpApplicationWithExternalServiceProvider application,
            IServiceProvider serviceProvider,
            DicomCompressService dicomCompressService)
        {
            _application = application;
            _serviceProvider = serviceProvider;
            _dicomCompressService = dicomCompressService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _application.Initialize(_serviceProvider);
            await Task.Delay(3000, cancellationToken);
            await _dicomCompressService.CompressAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _application.Shutdown();

            return Task.CompletedTask;
        }
    }
}
