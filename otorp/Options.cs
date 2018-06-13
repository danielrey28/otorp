using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace otorp
{
    internal class Options
    {
        [Option('i', "Input Directory", Required = true, HelpText = "Directory where input files to be processed exist.")]
        public string InputDirectory { get; set; }

        [Option('o', "Output Directory", Required = false, HelpText = "Directory where generated files should go. (Default = Same as Input Directory).")]
        public string OutputDirectory { get; set; }

        [Option('p', "package", Required = false, HelpText = "Specify a package name (Optional)")]
        public string Package { get; set; }

        [Option('n', "namespace", Required = false, HelpText = "Specify a namespace (Optional)")]
        public string Namespace { get; set; }

        [Option('w', "overwrite", Default = false, Required = false, HelpText = "Overwrite existing proto files (Optional)")]
        public bool Overwrite { get; set; }
    }
}
