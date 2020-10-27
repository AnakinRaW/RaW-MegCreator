using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;

namespace MegCreatorCLI
{
    internal class Bootstrapper
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<PackOptions>(args)
                .WithParsed(ExecInternal)
                .WithNotParsed(HandleParseErrorsInternal);
        }

        private static void ExecInternal(PackOptions opts)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, opts);
            var application = new Application(serviceCollection.BuildServiceProvider());
            Environment.ExitCode = application.Run();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection, PackOptions options)
        {
            serviceCollection.AddLogging(config =>
                {
#if DEBUG
                    config.AddDebug();
#endif
                    config.AddConsole();
                })
                .Configure<LoggerFilterOptions>(options =>
                {
#if DEBUG
                    options.AddFilter<DebugLoggerProvider>(null, LogLevel.Trace);
#endif
                    options.AddFilter<ConsoleLoggerProvider>(null, LogLevel.Warning);
                });
            var lsp = serviceCollection.BuildServiceProvider();
            serviceCollection.AddTransient<IPacker, Packer>(s => new Packer(options, lsp.GetService<ILoggerFactory>().CreateLogger<IPacker>()));
        }

        private static void HandleParseErrorsInternal(IEnumerable<Error> errs)
        {
            var errors = errs as Error[] ?? errs.ToArray();
            if (errors.OfType<HelpVerbRequestedError>().Any() || errors.OfType<HelpRequestedError>().Any())
            {
                Environment.ExitCode = 0;
                return;
            }
            Environment.ExitCode = 64;
        }
    }
}