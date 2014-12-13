using CommandLine;
using CommandLine.Text;

namespace BackwardCompatibilityChecker
{
    class Settings
    {
        [Option("old", Required = true, HelpText = "Path to the directory with old binary files")]
        public string OldPath { get; set; }

        [Option("new", Required = true, HelpText = "Path to the directory with new binary files")]
        public string NewPath { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("Backward Compatibility Checker Tool"),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddOptions(this);
            return help;
        }
    }
}
