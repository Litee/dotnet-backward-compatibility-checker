using System;

namespace BackwardCompatibilityChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Out.WriteLine("INFO: Starting with arguments {0}", string.Join(" ", args));
            var settings = new Settings();
            var result = CommandLine.Parser.Default.ParseArguments(args, settings);
            if (result)
            {
                new DirectoriesDiff().Execute(settings.OldPath, settings.NewPath);
            }
        }
    }
}
