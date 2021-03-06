using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BackwardCompatibilityChecker.Introspection.Types;
using Mono.Cecil;

namespace BackwardCompatibilityChecker.Introspection.Query
{
    public class MethodQuery : BaseQuery
    {
        protected internal bool? myIsVirtual;

        public static MethodQuery AllMethods
        {
            get { return new MethodQuery(); }
        }

        const string All = " * *(*)";

        public static MethodQuery ProtectedMethods
        {
            get { return new MethodQuery("protected " + All); }
        }

        public static MethodQuery InternalMethods
        {
            get { return new MethodQuery("internal " + All); }
        }

        public static MethodQuery PublicMethods
        {
            get { return new MethodQuery("public " + All); }
        }

        public static MethodQuery PrivateMethods
        {
            get { return new MethodQuery("private " + All); }
        }

        internal Regex ReturnTypeFilter
        {
            get; set;
        }

        internal List<KeyValuePair<Regex,string>> ArgumentFilters
        {
            get; set;
        }

        static char[] myArgTrimChars = new char[] { '[', ']', ',' };

        /// <summary>
        /// Create a method query which matches every method
        /// </summary>
        public MethodQuery():this("*")
        {
        }

        /// <summary>
        /// Create a new instance of a Query to match for specific methods for a given type.
        /// </summary>
        /// <remarks>The query format can be a simple string like
        /// * // get everything
        /// public void Function(int firstArg, bool secondArg)  // match specfic method
        /// public * *( * ) // match all public methods
        /// protected * *(* a) // match all protected methods with one parameter
        /// </remarks>
        /// <param name="methodQuery">The method query.</param>
        public MethodQuery(string methodQuery)
            : base(methodQuery)
        {

            // Return everything if no filter is set
            if (methodQuery.Trim() == "*")
            {
                return;
            }

            // Get cached instance
            this.Parser = MethodDefParser;

            // otherwise we expect a filter query that looks like a function definition
            Match m = this.Parser.Match(methodQuery.Trim());

            if (!m.Success)
            {
                throw new ArgumentException(String.Format("Invalid method query: \"{0}\". The method query must be of the form <modifier> <return type> <function name>(<arguments>) e.g. public void F(*) match all public methods with name F with 0 or more arguments, or public * *(*) match any public method.", methodQuery));
            }

            this.CreateReturnTypeFilter(m);

            this.NameFilter = m.Groups["funcName"].Value;
            int idx = this.NameFilter.IndexOf('<');
            if( idx != -1 )
                this.NameFilter = this.NameFilter.Substring(0, idx );

            if( String.IsNullOrEmpty(this.NameFilter) )
            {
                this.NameFilter = null;
            }
            
            this.ArgumentFilters = this.InitArgumentFilter(m.Groups["args"].Value);

            this.SetModifierFilter(m);
        }

        private void CreateReturnTypeFilter(Match m)
        {
            string filter = m.Groups["retType"].Value.Replace(" ", "");

            if (!String.IsNullOrEmpty(filter))
            {
                this.ReturnTypeFilter = this.CreateRegularExpressionFromTypeString(filter); 
            }
        }

        protected override void SetModifierFilter(Match m)
        {
            base.SetModifierFilter(m);
            this.myIsVirtual = this.Captures(m, "virtual");
        }

        protected bool MatchMethodModifiers(MethodDefinition method)
        {
            bool lret = true;

            if (this.MyIsPublic.HasValue)
                lret = method.IsPublic == this.MyIsPublic;
            if (lret && this.MyIsInternal.HasValue)
                lret = method.IsAssembly == this.MyIsInternal;
            if (lret && this.MyIsPrivate.HasValue)
                lret = method.IsPrivate == this.MyIsPrivate;
            if (lret && this.MyIsProtectedInternal.HasValue)
                lret = method.IsFamilyOrAssembly == this.MyIsProtectedInternal;
            if (lret && this.MyIsProtected.HasValue)
                lret = method.IsFamily == this.MyIsProtected;
            if (lret && this.myIsVirtual.HasValue)
                lret = method.IsVirtual == this.myIsVirtual;
            if (lret && this.MyIsStatic.HasValue)
                lret = method.IsStatic == this.MyIsStatic;

            return lret;
        }

        internal List<KeyValuePair<Regex, string>> InitArgumentFilter(string argFilter)
        {
            if (argFilter == null || argFilter == "*")
                return null;

            // To query for void methods
            if (argFilter == "")
                return new List<KeyValuePair<Regex, string>>();

            int inGeneric = 0;

            bool bIsType = true;
            List<KeyValuePair<Regex, string>> list = new List<KeyValuePair<Regex, string>>();
            StringBuilder curThing = new StringBuilder();
            string curType = null;
            string curArgName = null;

            char prev = '\0';
            char current;
            for (int i = 0; i < argFilter.Length; i++)
            {
                current = argFilter[i];

                if( current != ' ' )
                    curThing.Append(current);

                if ('<' == current)
                {
                    inGeneric++;
                }
                else if ('>' == current)
                {
                    inGeneric--;
                }

                if (inGeneric > 0)
                    continue;

                if (i > 0)
                    prev = argFilter[i - 1];

                // ignore subsequent spaces
                if(' ' == current && prev == ' ')
                {
                    continue;
                }

                // Got end of file argument name
                if (',' == current && curThing.Length > 0)
                {
                    curThing.Remove(curThing.Length - 1, 1);
                    curArgName = curThing.ToString().Trim();
                    curThing.Length = 0;

                    if (curType == null || curArgName == null)
                    {
                        throw new ArgumentException(
                            String.Format("Method argument filter is of wrong format: {0}", argFilter));
                    }

                    list.Add(this.AssignArrayBracketsToTypeName(curType, curArgName));
                    curType = null;
                    curArgName = null;

                    bIsType = true;
                }

                if( current == ' ' && curThing.Length > 0 && bIsType != false) 
                {
                    curType = GenericTypeMapper.ConvertClrTypeNames(curThing.ToString().Trim());
                    curThing.Length = 0;
                    bIsType = false;
                }
            }

            if (curType != null)
            {
                list.Add( this.AssignArrayBracketsToTypeName(curType, curThing.ToString().Trim()) );
            }


            return list;
        }

        KeyValuePair<Regex, string> AssignArrayBracketsToTypeName(string typeName, string argName)
        {
            string newTypeName = typeName;
            string newArgName = argName;

            if (argName.StartsWith("["))
            {
                newTypeName += argName.Substring(0, argName.LastIndexOf(']') + 1);
                newArgName = newArgName.Trim(myArgTrimChars);
            }

            newArgName = this.PrependStarToFilter(newArgName);
            Regex typeFilter = this.CreateRegularExpressionFromTypeString(newTypeName);

            return new KeyValuePair<Regex, string>(typeFilter, newArgName);
        }

        private Regex CreateRegularExpressionFromTypeString(string newTypeName)
        {
            newTypeName = this.CreateRegexFilterFromTypeName(newTypeName);
            if (newTypeName.StartsWith("*"))
            {
                newTypeName = "." + newTypeName;
            }

            newTypeName = GenericTypeMapper.TransformGenericTypeNames(newTypeName, this.CreateRegexFilterFromTypeName);

            newTypeName = Regex.Escape(newTypeName);
            // unescape added wild cards
            newTypeName = newTypeName.Replace("\\.\\*", ".*");
            return new Regex(newTypeName, RegexOptions.IgnoreCase); ;
        }

        string CreateRegexFilterFromTypeName(string filterstr)
        {
            if (!String.IsNullOrEmpty(filterstr))
            {
                if (!filterstr.StartsWith(".*") && !filterstr.StartsWith("*"))
                {
                    return ".*" + filterstr;
                }
            }
            return filterstr;
        }

        string PrependStarToFilter(string filterstr)
        {
            if (!String.IsNullOrEmpty(filterstr))
            {
                if (!filterstr.StartsWith("*"))
                {
                    return "*" + filterstr;
                }
            }
            return filterstr;
        }

        internal bool MatchReturnType(MethodDefinition method)
        {
            if (this.ReturnTypeFilter == null )
            {
                return true;
            }

            return this.ReturnTypeFilter.IsMatch(method.MethodReturnType.ReturnType.FullName);
        }

        bool IsArgumentMatch(Regex typeFilter, string argNameFilter, string typeName, string argName)
        {
            Match m = typeFilter.Match(typeName);
            bool lret = m.Success;
            if (lret)
            {
                lret = Matcher.MatchWithWildcards(argNameFilter, argName, StringComparison.OrdinalIgnoreCase);
            }

            return lret;
        }


        internal bool MatchArguments(MethodDefinition method)
        {
            // Query all methods regardless number of parameters
            if (this.ArgumentFilters == null)
                return true;

            if (this.ArgumentFilters.Count != method.Parameters.Count)
                return false;

            for (int i = 0; i < this.ArgumentFilters.Count; i++)
            {
                ParameterDefinition curDef = method.Parameters[i];
                KeyValuePair<Regex, string> curFilters = this.ArgumentFilters[i];

                if (!this.IsArgumentMatch(curFilters.Key, curFilters.Value, curDef.ParameterType.FullName, curDef.Name))
                {
                    return false;
                }
            }

            return true;
        }



        public MethodDefinition GetSingleMethod(TypeDefinition type)
        {
            var matches = this.GetMethods(type);
            if (matches.Count > 1)
                throw new InvalidOperationException(String.Format("Got more than one matching method: {0}", matches.Count));

            if (matches.Count == 0)
                return null;

            return matches[0];
        }

        public virtual List<MethodDefinition> GetMethods(TypeDefinition type)
        {
            var matchingMethods = new List<MethodDefinition>();
            foreach (MethodDefinition method in type.Methods)
            {
                if (this.Match(type,method))
                    matchingMethods.Add(method);
            }

            // ctors are normal methods to us
            foreach (MethodDefinition method in type.Methods.Where(x => x.IsConstructor))
            {
                if (this.Match(type, method))
                    matchingMethods.Add(method);
            }

            return matchingMethods;
        }


        internal bool Match(TypeDefinition type, MethodDefinition method)
        {
            bool lret = this.MatchMethodModifiers(method);

            if (lret)
            {
                lret = this.MatchName(method.Name);
                if (method.Name == ".ctor")
                {
                    lret = this.MatchName(method.DeclaringType.Name);
                }
            }

            if (lret)
            {
                lret = this.MatchReturnType(method);
            }

            if (lret)
            {
                lret = this.MatchArguments(method);
            }

            if (lret)
            {
                lret = this.IsNoEventMethod(type, method);
            }

            return lret;
        }

        private bool IsNoEventMethod(TypeDefinition type, MethodDefinition method)
        {
            bool lret = true;
            if (method.IsSpecialName) // Is usually either a property or event add/remove method
            {
                foreach (EventDefinition ev in type.Events)
                {
                    if (ev.AddMethod.IsEqual(method) ||
                        ev.RemoveMethod.IsEqual(method))
                    {
                        lret = false;
                        break;
                    }
                }
            }

            return lret;
        }
    }
}
