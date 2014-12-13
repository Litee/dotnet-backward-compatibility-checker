using System;
using System.Text.RegularExpressions;

namespace BackwardCompatibilityChecker.Introspection.Query
{
    public class BaseQuery
    {
        protected bool? MyIsPrivate;
        protected bool? MyIsPublic;
        protected bool? MyIsInternal;
        protected bool? MyIsProtected;
        protected bool? MyIsProtectedInternal;
        protected bool? MyIsStatic;


        static Regex myEventQueryParser;

        // Common Regular expression part shared by the different queries
        const string CommonModifiers = "!?static +|!?public +|!?protected +internal +|!?protected +|!?internal +|!?private +";

        internal static Regex EventQueryParser
        {
            get
            {
                if (myEventQueryParser == null)
                {
                    myEventQueryParser = new Regex(
                        "^ *(?<modifiers>!?virtual +|event +|" + CommonModifiers + ")*" +
                        @" *(?<eventType>[^ ]+(<.*>)?) +(?<eventName>[^ ]+) *$");
                }

                return myEventQueryParser;
            }
        }

        static Regex _myFieldQueryParser;
        internal static Regex FieldQueryParser
        {
            get
            {
                if (_myFieldQueryParser == null)
                {
                    _myFieldQueryParser = new Regex(
                        " *(?<modifiers>!?nocompilergenerated +|!?const +|!?readonly +|" + CommonModifiers + ")*" +
                        @" *(?<fieldType>[^ ]+(<.*>)?) +(?<fieldName>[^ ]+) *$");
                }

                return _myFieldQueryParser;
            }
        }

        static Regex _myMethodDefParser;
        internal static Regex MethodDefParser
        {
            get
            {
                if (_myMethodDefParser == null)
                {
                    _myMethodDefParser = new Regex
                     (
                        @" *(?<modifiers>!?virtual +|" + CommonModifiers + ")*" +
                        @"(?<retType>.*<.*>( *\[\])?|[^ (\)]*( *\[\])?) +(?<funcName>.+)\( *(?<args>.*?) *\) *"
                     );
                }

                return _myMethodDefParser;
            }
        }

        protected Regex Parser
        {
            get;
            set;
        }

        protected string NameFilter
        {
            get;
            set;
        }

        protected BaseQuery(string query)
        {
            if (String.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException("query");
            }
        }


        protected virtual internal bool IsMatch(Match m, string key)
        {
            return m.Groups[key].Success;
        }

        protected virtual internal bool? Captures(Match m, string value)
        {
            string notValue = "!" + value;
            foreach (Capture capture in m.Groups["modifiers"].Captures)
            {
                if (value == capture.Value.TrimEnd())
                    return true;
                if (notValue == capture.Value.TrimEnd())
                    return false;
            }

            return null;
        }

        protected string Value(Match m, string groupName)
        {
            return m.Groups[groupName].Value;
        }

        protected virtual void SetModifierFilter(Match m)
        {
            this.MyIsProtected = this.Captures(m, "protected");
            this.MyIsInternal = this.Captures(m, "internal");
            this.MyIsProtectedInternal = this.Captures(m, "protected internal");
            this.MyIsPublic = this.Captures(m, "public");
            this.MyIsPrivate = this.Captures(m, "private");
            this.MyIsStatic = this.Captures(m, "static");
        }

        protected virtual bool MatchName(string name)
        {
            if (String.IsNullOrEmpty(this.NameFilter) || this.NameFilter == "*")
            {
                return true;
            }

            return Matcher.MatchWithWildcards(this.NameFilter, name, StringComparison.OrdinalIgnoreCase);
        }

        int CountChars(char searchChar, string str)
        {
            int ret = 0;
            foreach (char c in str)
            {
                if (c == searchChar)
                    ret++;
            }

            return ret;
        }
    }
}
