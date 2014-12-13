using System;
using System.Collections.Generic;
using BackwardCompatibilityChecker.Introspection.Query;
using BackwardCompatibilityChecker.Introspection.Types;
using Mono.Cecil;

namespace BackwardCompatibilityChecker.Introspection.Diff
{
    public class TypeDiff
    {
        public TypeDefinition TypeV1 { get; private set; }
        public TypeDefinition TypeV2 { get; private set; }

        public DiffCollection<MethodDefinition> Methods           { get; private set; }
        public DiffCollection<EventDefinition>  Events            { get; private set; }
        public DiffCollection<FieldDefinition>  Fields            { get; private set; }
        public DiffCollection<TypeReference>    Interfaces        { get; private set; }

        public bool HasChangedBaseType                         { get; private set; }

        static readonly TypeDefinition noType = new TypeDefinition("noType",null,TypeAttributes.Class, null);

        static readonly TypeDiff myNone = new TypeDiff(noType, noType);

        /// <summary>
        /// Default return object when the diff did not return any results
        /// </summary>
        public static TypeDiff None
        {
            get
            {
                return myNone;
            }
        }

        /// <summary>
        /// Checks if the type has changes
        /// On type level
        /// Base Types, implemented interfaces, generic parameters
        /// On method level
        /// Method modifiers, return type, generic parameters, parameter count, parameter types (also generics)
        /// On field level
        /// Field types
        /// </summary>
        /// <param name="typeV1">The type v1.</param>
        /// <param name="typeV2">The type v2.</param>
        /// <param name="diffQueries">The diff queries.</param>
        /// <returns></returns>
        public static TypeDiff GenerateDiff(TypeDefinition typeV1, TypeDefinition typeV2, QueryAggregator diffQueries)
        {
            if (typeV1 == null)
                throw new ArgumentNullException("typeV1");
            if (typeV2 == null)
                throw new ArgumentNullException("typeV2");
            if (diffQueries == null || diffQueries.FieldQueries.Count == 0
                || diffQueries.MethodQueries.Count == 0)
            {
                throw new ArgumentException("diffQueries was null or the method or field query list was emtpy. This will not result in a meaningful diff result");
            }

            var diff = new TypeDiff(typeV1, typeV2);

            diff.DoDiff(diffQueries);

            if (!diff.HasChangedBaseType &&
                 diff.Events.Count == 0 &&
                 diff.Fields.Count == 0 &&
                 diff.Interfaces.Count == 0 &&
                 diff.Methods.Count == 0)
            {
                return TypeDiff.None;
            }

            return diff;
        }

        TypeDiff(TypeDefinition v1, TypeDefinition v2)
        {
            this.TypeV1 = v1;
            this.TypeV2 = v2;

            this.Methods = new DiffCollection<MethodDefinition>();
            this.Events = new DiffCollection<EventDefinition>();
            this.Fields = new DiffCollection<FieldDefinition>();
            this.Interfaces = new DiffCollection<TypeReference>();
        }

        bool IsSameBaseType(TypeDefinition t1, TypeDefinition t2)
        {
            if (Object.ReferenceEquals(t1,null) && Object.ReferenceEquals(t2,null))
            {
                return true;
            }

            if ( (t1 == null) || (t2 == null))
            {
                return false;
            }

            if (t1.BaseType == null && t2.BaseType == null)
                return true;

            // compare base type
            if(t1.BaseType == null || t2.BaseType == null)
            {
                return false;
            }

            return t1.BaseType.FullName == t2.BaseType.FullName;
        }

        void DoDiff(QueryAggregator diffQueries)
        {
            // Interfaces have no base type
            if( !this.TypeV1.IsInterface )
            {
                this.HasChangedBaseType = !this.IsSameBaseType(this.TypeV1,this.TypeV2);
            }

            this.DiffImplementedInterfaces();
            this.DiffFields(diffQueries);
            this.DiffMethods(diffQueries);
            this.DiffEvents(diffQueries);
        }

        private void DiffImplementedInterfaces()
        {

            // search for removed interfaces
            foreach (TypeReference baseV1 in this.TypeV1.Interfaces)
            {
                bool bFound = false;
                foreach (TypeReference baseV2 in this.TypeV2.Interfaces)
                {
                    if (baseV2.IsEqual(baseV1))
                    {
                        bFound = true;
                        break;
                    }
                }
                if (!bFound)
                {
                    this.Interfaces.Add(new DiffResult<TypeReference>(baseV1, new DiffOperation(false)));
                }
            }

            // search for added interfaces
            foreach (TypeReference baseV2 in this.TypeV2.Interfaces)
            {
                bool bFound = false;
                foreach (TypeReference baseV1 in this.TypeV1.Interfaces)
                {
                    if (baseV2.IsEqual(baseV1))
                    {
                        bFound = true;
                        break;
                    }
                }
                if (!bFound)
                {
                    this.Interfaces.Add(new DiffResult<TypeReference>(baseV2, new DiffOperation(true)));
                }
            }
        }

        private void DiffFields(QueryAggregator diffQueries)
        {
            List<FieldDefinition> fieldsV1 = diffQueries.ExecuteAndAggregateFieldQueries(this.TypeV1);
            List<FieldDefinition> fieldsV2 = diffQueries.ExecuteAndAggregateFieldQueries(this.TypeV2);

            var fieldDiffer = new ListDiffer<FieldDefinition>(this.CompareFieldsByTypeAndName);
            fieldDiffer.Diff(fieldsV1, fieldsV2,
                addedField =>
                {
                    this.Fields.Add(new DiffResult<FieldDefinition>(addedField, new DiffOperation(true)));
                },
                removedField =>
                {
                    this.Fields.Add(new DiffResult<FieldDefinition>(removedField, new DiffOperation(false)));
                });
        }

        bool CompareFieldsByTypeAndName(FieldDefinition fieldV1, FieldDefinition fieldV2)
        {
            return fieldV1.IsEqual(fieldV2);
        }

        private void DiffMethods(QueryAggregator diffQueries)
        {
            var methodsV1 = diffQueries.ExecuteAndAggregateMethodQueries(this.TypeV1);
            var methodsV2 = diffQueries.ExecuteAndAggregateMethodQueries(this.TypeV2);

            var differ = new ListDiffer<MethodDefinition>(this.CompareMethodByNameAndTypesIncludingGenericArguments);

            differ.Diff(methodsV1, methodsV2,
                added =>
                {
                    this.Methods.Add( new DiffResult<MethodDefinition>(added, new DiffOperation(true) ));
                },
                removed =>
                {
                        this.Methods.Add( new DiffResult<MethodDefinition>(removed, new DiffOperation(false)) );
                }
                );
        }

        bool CompareMethodByNameAndTypesIncludingGenericArguments(MethodDefinition m1, MethodDefinition m2)
        {
            return m1.IsEqual(m2);
        }

        private void DiffEvents(QueryAggregator diffQueries)
        {
            List<EventDefinition> eventsV1 = diffQueries.ExecuteAndAggregateEventQueries(this.TypeV1);
            List<EventDefinition> eventsV2 = diffQueries.ExecuteAndAggregateEventQueries(this.TypeV2);

            var differ = new ListDiffer<EventDefinition>(this.CompareEvents);

            differ.Diff(eventsV1, eventsV2,
                added =>
                {
                    this.Events.Add(new DiffResult<EventDefinition>(added, new DiffOperation(true)));
                },
                 removed =>
                 {
                     this.Events.Add( new DiffResult<EventDefinition>(removed, new DiffOperation(false)));
                 });
        }

        bool CompareEvents(EventDefinition evV1, EventDefinition evV2)
        {
            return evV1.IsEqual(evV2);
        }

        public override string ToString()
        {
            return String.Format("Type: {0}, Changed Methods: {1}, Fields: {2}, Events: {3}, Interfaces: {4}",
                this.TypeV1, this.Methods.Count, this.Fields.Count, this.Events.Count, this.Interfaces.Count);
        }

    }
}
