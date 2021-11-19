using CommandLine;

namespace NzCovidPass.Console
{
    internal class Options
    {
        [Option('p', "pass", Required = true, HelpText = "Pass to verify.")]
        public string Pass { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Enable verbose logging.")]
        public bool Verbose { get; set; }
    }
}
