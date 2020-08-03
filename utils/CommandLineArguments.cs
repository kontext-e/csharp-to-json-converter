using CommandLine;

namespace csharp_to_json_converter.utils
{
    public class CommandLineArguments
    {
        [Option('i', "input", Required = true, HelpText = "Input directory to be processed.")]
        public string InputDirectory { get; set; }
        
        [Option('o', "output", Required = true, HelpText = "Output directory.")]
        public string OutputDirectory { get; set; }
    }
}