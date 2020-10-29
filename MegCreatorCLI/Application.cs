using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MegCreatorCLI
{
    internal class Application
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger? _logger;

        public Application(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Application>();
            _logger?.LogTrace("Application initialized successfully.");
        }

        public int Run()
        {
            _logger?.LogInformation("Running application in RUN mode.");
            using var packer = _serviceProvider.GetService<IPacker>();
            try
            {
                packer.Pack();
            }
            catch (Exception e)
            {
                _logger?.LogError($"Unable to pack a MEG file: {e.Message}", e);
                return 1;
            }
            return 0;
        }
    }
}
