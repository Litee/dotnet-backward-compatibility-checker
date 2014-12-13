using System.Collections.Generic;
using System.Diagnostics;
using Mono.Cecil;

namespace BackwardCompatibilityChecker.Introspection.Diff
{
    [DebuggerDisplay("Add {AddedRemovedTypes.AddedCount} Remove {AddedRemovedTypes.RemovedCount} Changed {ChangedTypes.Count}")]
    public class AssemblyDiffCollection
    {
        public readonly List<TypeDefinition> AddedTypes = new List<TypeDefinition>();
        public readonly List<TypeDefinition> RemovedTypes = new List<TypeDefinition>();
        public readonly List<TypeDiff> ChangedTypes = new List<TypeDiff>();
    }
}
