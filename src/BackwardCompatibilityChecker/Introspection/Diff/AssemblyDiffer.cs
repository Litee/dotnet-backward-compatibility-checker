using System;
using System.Collections.Generic;
using BackwardCompatibilityChecker.Introspection.Query;
using BackwardCompatibilityChecker.Introspection.Types;
using Mono.Cecil;

namespace BackwardCompatibilityChecker.Introspection.Diff
{
    public class AssemblyDiffer
    {
        readonly AssemblyDefinition myV1;
        readonly AssemblyDefinition myV2;
        readonly AssemblyDiffCollection myDiff = new AssemblyDiffCollection();

        public AssemblyDiffer(AssemblyDefinition v1, AssemblyDefinition v2)
        {
            if (v1 == null)
                throw new ArgumentNullException("v1");
            if (v2 == null)
                throw new ArgumentNullException("v2");

            this.myV1 = v1;
            this.myV2 = v2;
        }

        void OnAddedType(TypeDefinition type)
        {
            this.myDiff.AddedTypes.Add(type);
        }

        void OnRemovedType(TypeDefinition type)
        {
            this.myDiff.RemovedTypes.Add(type);
        }

        public AssemblyDiffCollection GenerateTypeDiff(QueryAggregator queries)
        {
            if (queries == null || queries.TypeQueries.Count == 0)
            {
                throw new ArgumentNullException("queries");
            }

            List<TypeDefinition> typesV1 = queries.ExeuteAndAggregateTypeQueries(this.myV1);
            List<TypeDefinition> typesV2 = queries.ExeuteAndAggregateTypeQueries(this.myV2);

            var differ = new ListDiffer<TypeDefinition>( this.ShallowTypeComapare );

            differ.Diff(typesV1, typesV2, this.OnAddedType, this.OnRemovedType);

            this.DiffTypes(typesV1, typesV2, queries);

            return this.myDiff;
        }

        bool ShallowTypeComapare(TypeDefinition v1, TypeDefinition v2)
        {
            return v1.FullName == v2.FullName;
        }


        private void DiffTypes(IEnumerable<TypeDefinition> typesV1, List<TypeDefinition> typesV2, QueryAggregator queries)
        {
            foreach (var typeV1 in typesV1)
            {
                TypeDefinition typeV2 = this.GetTypeByDefinition(typeV1, typesV2);
                if (typeV2 != null)
                {
                    TypeDiff diffed = TypeDiff.GenerateDiff(typeV1, typeV2, queries);
                    if (diffed != TypeDiff.None)
                    {
                        this.myDiff.ChangedTypes.Add(diffed);
                    }
                }
            }
        }

        TypeDefinition GetTypeByDefinition(TypeDefinition search, IEnumerable<TypeDefinition> types)
        {
            foreach (var type in types)
            {
                if (type.IsEqual(search))
                    return type;
            }

            return null;
        }
    }
}
