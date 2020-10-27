using System.Collections.Generic;
using CommandLine;

namespace MegCreatorCLI
{
    internal class PackOptions
    {
        [Option('i', "inputLocations", Required = false, HelpText = "A list of file locations, which shall get packed.")]
        public IEnumerable<string> InputLocations { get; set; }

        [Option('o', "output", Required = true, HelpText = "The name of the .meg which will get created." + 
                                                           " The parameter also may be an relative or absolute path.")]
        public string Output { get; set; }

        [Option('s', "includeSubDirs", Required = false, Default = false, HelpText = "If any input is a directory this flag ensures to also include its subdirectories.")]
        public bool IncludeSubDirectories { get; set; }

        [Option('w', "workingDir", Required = false, Default = null, HelpText = "Sets the root location for relative paths used by input and output." +
            " If not specified it defaults to the path of this executable. If this path is relative it's based on the path of the path of this executable.")]
        public string? WorkingDirectory { get; set; }
}
}
