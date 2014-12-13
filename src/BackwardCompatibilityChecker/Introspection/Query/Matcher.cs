using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BackwardCompatibilityChecker.Introspection.Query
{
    /// <summary>
    /// Partial String matcher class which supports wildcards
    /// </summary>
    internal static class Matcher
    {
        static readonly char[] MyNsTrimChars = { ' ', '*', '\t' };
        const string EscapedStar = "magic_star";

        // Cache filter string regular expressions for later reuse
        private static readonly Dictionary<string, Regex> myFilter2Regex = new Dictionary<string, Regex>();

        /// <summary>
        /// Check if a given test string does match the pattern specified by the filterString. Besides
        /// normal string comparisons for the patterns *xxx, xxx*, *xxx* which are mapped to String.EndsWith,
        /// String.StartsWith and String.Contains are regular expressions used if the pattern is more complex
        /// like *xx*bbb.
        /// </summary>
        /// <param name="filterString">Filter string. A filter string of null or * will match any testString. If the teststring is null it will never match anything.</param>
        /// <param name="testString">String to check</param>
        /// <param name="compMode">String Comparison mode</param>
        /// <returns>true if the teststring does match, false otherwise.</returns>
        public static bool MatchWithWildcards(string filterString, string testString, StringComparison compMode)
        {
            if (filterString == null || filterString == "*")
                return true;

            if (testString == null)
            {
                return false;
            }


            if (IsRegexMatchNecessary(filterString))
            {
                return IsMatch(filterString, testString, compMode);
            }

            bool bMatchEnd = filterString.StartsWith("*", compMode);

            bool bMatchStart = filterString.EndsWith("*", compMode);

            string filterSubstring = filterString.Trim(MyNsTrimChars);

            if (bMatchStart && bMatchEnd)
            {
                if (compMode == StringComparison.OrdinalIgnoreCase ||
                    compMode == StringComparison.InvariantCultureIgnoreCase)
                {
                    return testString.ToLower().Contains(filterSubstring.ToLower());
                }
                return testString.Contains(filterSubstring);
            }

            if (bMatchStart)
            {
                return testString.StartsWith(filterSubstring,compMode);
            }

            if (bMatchEnd)
            {
                return testString.EndsWith(filterSubstring,compMode);
            }

            return String.Compare(testString,filterSubstring, compMode) == 0;
        }

  
        // Check if * occurs inside filter string and not only at start or end
        private static bool IsRegexMatchNecessary(string filter)
        {
            int start = Math.Min(1, Math.Max(filter.Length-1,0));
            int len = Math.Max(filter.Length - 2, 0);
            return filter.IndexOf("*", start, len) != -1;
        }

        private static bool IsMatch(string filter, string testString, StringComparison compMode)
        {
            Regex filterRex = GenerateRegexFromFilter(filter, compMode);
            return filterRex.IsMatch(testString);
        }


        private static Regex GenerateRegexFromFilter(string filter, StringComparison mode)
        {
            Regex result = null;

            if (!myFilter2Regex.TryGetValue(filter, out result))
            {
                string rex = Regex.Escape(filter.Replace("*", EscapedStar));
                rex = "^" + rex + "$";
                result = new Regex(rex.Replace(EscapedStar, ".*?"),
                                (   mode == StringComparison.CurrentCultureIgnoreCase ||
                                    mode == StringComparison.InvariantCultureIgnoreCase ||
                                    mode == StringComparison.OrdinalIgnoreCase
                                 ) ? RegexOptions.IgnoreCase : RegexOptions.None);
                myFilter2Regex[filter] = result;
            }

            return result;
        }
    }
}
