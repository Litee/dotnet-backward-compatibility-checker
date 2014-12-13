using System.Collections.Generic;

namespace BackwardCompatibilityChecker.Introspection.Query.UsageQueries
{
    public class MatchContext : Dictionary<string,object>
    {
        public const string MatchReason = "Match Reason";
        public const string MatchItem = "Match Item";

        public string Reason
        {
            get
            {
                object lret = "";
                if (!this.TryGetValue(MatchReason, out lret))
                {
                    lret = "";
                }

                return lret.ToString();
            }
        }

        public string Item
        {
            get
            {
                object lret;
                if (!this.TryGetValue(MatchItem, out lret))
                {
                    lret = "";
                }

                return lret.ToString();
            }
        }

        public MatchContext(string matchReason, string matchItem)
        {
            this[MatchReason] = matchReason;
            this[MatchItem] = matchItem;
        }

        public MatchContext()
        {
        }
    }
}
