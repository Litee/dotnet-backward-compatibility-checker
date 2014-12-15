using System.IO;
using ApprovalTests;
using ApprovalTests.Reporters;
using BackwardCompatibilityChecker.Introspection.Diff;
using BackwardCompatibilityChecker.Introspection.Query;
using Mono.Cecil;
using NUnit.Framework;

namespace BackwardCompatibilityChecker.Tests
{
    [UseReporter(typeof(WinMergeReporter))]
    [TestFixture]
    public class AssemblyDifferTests
    {
        [Test]
        public void ShouldFindDifferences()
        {
            var assemblyV1 = AssemblyDefinition.ReadAssembly(@"..\..\..\OldAssembly\bin\Debug\OldAssembly.dll");
            var assemblyV2 = AssemblyDefinition.ReadAssembly(@"..\..\..\NewAssembly\bin\Debug\OldAssembly.dll");
            var assemblyDiffer = new AssemblyDiffer(assemblyV1, assemblyV2);
            var assemblyDiffCollection = assemblyDiffer.GenerateTypeDiff(QueryAggregator.PublicApiQueries);
            var stringWriter = new StringWriter();
            new DiffPrinter(stringWriter).Print(assemblyDiffCollection);
            Approvals.Verify(stringWriter.ToString());
        }
    }
}