using System.Collections.Generic;
using System.Linq;
using BackwardCompatibilityChecker.Introspection.Types;
using Mono.Cecil;

namespace BackwardCompatibilityChecker.Introspection.Query
{
    class TypeNameComparer : IEqualityComparer<TypeDefinition>
    {
        #region IEqualityComparer<TypeDefinition> Members

        public bool Equals(TypeDefinition x, TypeDefinition y)
        {
            return x.FullName == y.FullName;
        }

        public int GetHashCode(TypeDefinition obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion
    }

    class MethodComparer : IEqualityComparer<MethodDefinition>
    {

        #region IEqualityComparer<MethodDefinition> Members

        public bool Equals(MethodDefinition x, MethodDefinition y)
        {
            return x.IsEqual(y);
        }

        public int GetHashCode(MethodDefinition obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion
    }

    class FieldComparer : IEqualityComparer<FieldDefinition>
    {

        #region IEqualityComparer<FieldDefinition> Members

        public bool Equals(FieldDefinition x, FieldDefinition y)
        {
            return x.IsEqual(y);
        }

        public int GetHashCode(FieldDefinition obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion
    }

    class EventComparer : IEqualityComparer<EventDefinition>
    {
        #region IEqualityComparer<EventDefinition> Members

        public bool Equals(EventDefinition x, EventDefinition y)
        {
            return x.AddMethod.IsEqual(y.AddMethod);
        }

        public int GetHashCode(EventDefinition obj)
        {
            return obj.AddMethod.Name.GetHashCode();
        }

        #endregion
    }


    public class QueryAggregator
    {
        public readonly List<TypeQuery> TypeQueries = new List<TypeQuery>();
        public readonly List<MethodQuery> MethodQueries = new List<MethodQuery>();
        public readonly List<FieldQuery> FieldQueries = new List<FieldQuery>();
        public readonly List<EventQuery> EventQueries = new List<EventQuery>();

        /// <summary>
        /// Contains also internal types, fields and methods since the InteralsVisibleToAttribute
        /// can open visibility
        /// </summary>
        public static QueryAggregator PublicApiQueries
        {
            get
            {
                var agg = new QueryAggregator();

                agg.TypeQueries.Add(new TypeQuery(TypeQueryMode.ApiRelevant));

                agg.MethodQueries.Add( MethodQuery.PublicMethods );
                agg.MethodQueries.Add( MethodQuery.ProtectedMethods );

                agg.FieldQueries.Add( FieldQuery.PublicFields );
                agg.FieldQueries.Add( FieldQuery.ProtectedFields );

                agg.EventQueries.Add( EventQuery.PublicEvents );
                agg.EventQueries.Add( EventQuery.ProtectedEvents );

                return agg;
            }
        }

        public List<TypeDefinition> ExeuteAndAggregateTypeQueries(AssemblyDefinition assembly)
        {
            var result = new List<TypeDefinition>();
            foreach (var query in this.TypeQueries)
            {
                result.AddRange(query.GetTypes(assembly));
            }

            var distinctResults = result;
            if (this.TypeQueries.Count > 1)
            {
                distinctResults = result.Distinct(new TypeNameComparer()).ToList();
            }

            return distinctResults;
        }

        public List<MethodDefinition> ExecuteAndAggregateMethodQueries(TypeDefinition type)
        {
            List<MethodDefinition> methods = new List<MethodDefinition>();
            foreach (var query in this.MethodQueries)
            {
                methods.AddRange(query.GetMethods(type));
            }

            var distinctResults = methods;
            if (this.MethodQueries.Count > 1)
            {
                distinctResults = methods.Distinct(new MethodComparer()).ToList();
            }

            return distinctResults;
        }

        public List<FieldDefinition> ExecuteAndAggregateFieldQueries(TypeDefinition type)
        {
            List<FieldDefinition> fields = new List<FieldDefinition>();
            foreach (var query in this.FieldQueries)
            {
                fields.AddRange(query.GetMatchingFields(type));
            }

            var distinctResults = fields;
            if (this.FieldQueries.Count > 1)
            {
                distinctResults = fields.Distinct(new FieldComparer()).ToList();
            }

            return distinctResults;
        }

        public List<EventDefinition> ExecuteAndAggregateEventQueries(TypeDefinition type)
        {
            List<EventDefinition> ret = new List<EventDefinition>();

            foreach (var query in this.EventQueries)
            {
                ret.AddRange(query.GetMatchingEvents(type));
            }

            var distinctEvents = ret;
            if( this.EventQueries.Count > 1 )
            {
                distinctEvents = ret.Distinct(new EventComparer()).ToList();
            }

            return distinctEvents;
        }
    }
}
