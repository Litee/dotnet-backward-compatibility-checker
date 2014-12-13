using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BackwardCompatibilityChecker.Infrastructure;
using BackwardCompatibilityChecker.Introspection;
using BackwardCompatibilityChecker.Introspection.Diff;
using BackwardCompatibilityChecker.Introspection.Query;

namespace BackwardCompatibilityChecker
{
    class DirectoriesDiff
    {
        public void Execute(string pathToV1, string pathToV2)
        {
            Console.WriteLine("Compare from {0} against {1}", pathToV1, pathToV2);
            bool breakingChanges = false;

            var directoryInfo1 = new DirectoryInfo(pathToV1);
            var directoryInfo2 = new DirectoryInfo(pathToV2);

            var query1 = new HashSet<string>(directoryInfo1.GetFiles().Select(x => x.FullName).Where(x => x.ToLower().EndsWith(".dll")), new FileNameComparer());
            var query2 = new HashSet<string>(directoryInfo2.GetFiles().Select(x => x.FullName).Where(x => x.ToLower().EndsWith(".dll")), new FileNameComparer());

            var removedFiles = query1.Where(file1 => !query2.Any(file2 => string.Equals(Path.GetFileName(file1), Path.GetFileName(file2), StringComparison.OrdinalIgnoreCase)));
            if (removedFiles.Any())
            {
                Console.WriteLine("Removed {0} files", removedFiles.Count());
                foreach (string str in removedFiles)
                {
                    Console.WriteLine("\t{0}", Path.GetFileName(str));
                }
                breakingChanges = true;
            }

            // Get files which are present in one set and the other
            var filesThatExistInBothQueries = query1.Where(file1 => query2.Any(file2 => string.Equals(Path.GetFileName(file1), Path.GetFileName(file2), StringComparison.OrdinalIgnoreCase)));

            foreach (string fileName1 in filesThatExistInBothQueries)
            {

                if (fileName1.EndsWith(".XmlSerializers.dll", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Ignoring xml serializer dll {0}", fileName1);
                    continue;
                }

                string fileName2 = query2.First(x => String.Equals(Path.GetFileName(fileName1), Path.GetFileName(x), StringComparison.CurrentCultureIgnoreCase));

                var assemblyV1 = AssemblyLoader.LoadCecilAssembly(fileName1);
                var assemblyV2 = AssemblyLoader.LoadCecilAssembly(fileName2);

                if (assemblyV1 != null && assemblyV2 != null)
                {
                    var differ = new AssemblyDiffer(assemblyV1, assemblyV2);
                    var diff = differ.GenerateTypeDiff(QueryAggregator.PublicApiQueries);

                    breakingChanges = breakingChanges || diff.RemovedTypes.Count > 0;

                    // changed types
                    foreach (TypeDiff changedType in diff.ChangedTypes)
                    {
                        if (changedType.TypeV1.IsInterface)
                        {
                            breakingChanges = breakingChanges || changedType.Events.RemovedList.Any() || changedType.Methods.RemovedList.Any();
                        }
                        else if (changedType.TypeV1.IsEnum)
                        {
                            breakingChanges = breakingChanges || changedType.Fields.RemovedList.Any();
                        }
                        else
                        {
                            breakingChanges = breakingChanges || changedType.Events.RemovedList.Any() || changedType.Methods.RemovedList.Any() || changedType.Interfaces.RemovedList.Any() || changedType.HasChangedBaseType || changedType.Fields.RemovedList.Any();
                        }
                    }

                    var printer = new DiffPrinter(Console.Out);
                    printer.Print(diff);
                }
            }

            if (breakingChanges)
            {
                Console.WriteLine("Breaking changed detected!!!");
                Environment.Exit(-1);
            }
        }
    }
}
