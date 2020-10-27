using System;
using System.Collections.Generic;
using System.IO;
using Alamo.CLI;
using Microsoft.Extensions.Logging;

namespace MegCreatorCLI
{
    internal class Packer : IPacker
    {
        private readonly ILogger? _logger;
        public PackOptions Options { get; }

        private MegaFile MegFile { get; }

        public Packer(PackOptions options, ILogger? logger = null)
        {
            _logger = logger;
            Options = options ?? throw new ArgumentNullException(nameof(options));
            MegFile = new MegaFile();
        }

        public void Pack()
        {
            var realWorkingDirectory = Options.WorkingDirectory is null
                ? Directory.GetCurrentDirectory()
                : GetAbsolutePath(Options.WorkingDirectory);


            var directories = new HashSet<string>();
            foreach (var input in Options.InputLocations)
            {
                
            }
        }


        private  string GetAbsolutePath(string path)
        {
            if (Path.IsPathRooted(path))
                return path;
            _logger?.LogTrace($"Converting relative path '{path}' to absolute path.");
            return Path.Combine(Directory.GetCurrentDirectory(), path);
        }
    }
}
