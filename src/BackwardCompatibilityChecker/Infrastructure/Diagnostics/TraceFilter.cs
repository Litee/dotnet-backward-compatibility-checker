using System;
using System.Diagnostics;

namespace BackwardCompatibilityChecker.Infrastructure.Diagnostics
{
    internal class TraceFilter
    {
        internal TraceFilter Next;
        readonly int[] _myFilterHashes;
        const int MATCHANY = -1;  // Hash value that marks a *
        MessageTypes myMsgTypeFilter = MessageTypes.None;
        Level myLevelFilter;

        string _myFilter;

        protected TraceFilter()
        {
        }

        public TraceFilter(string typeFilter, MessageTypes msgTypeFilter, Level levelFilter, TraceFilter next)
        {
            if (String.IsNullOrEmpty(typeFilter))
            {
                throw new ArgumentException("typeFilter was null or empty");
            }

            this._myFilter = typeFilter;
            this.Next = next;
            this.myMsgTypeFilter = msgTypeFilter;
            this.myLevelFilter = levelFilter;

            string[] parts = typeFilter.Trim().ToLower().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            this._myFilterHashes = new int[parts.Length];
            Debug.Assert(parts.Length > 0, "Type filter parts should be > 0");
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "*")
                    this._myFilterHashes[i] = MATCHANY;
                else
                {
                    this._myFilterHashes[i] = parts[i].GetHashCode();
                }
            }
        }

        public virtual bool IsMatch(TypeHashes type, MessageTypes msgTypeFilter, Level level)
        {
            bool lret = ((level & this.myLevelFilter) != Level.None);

            if (lret)
            {
                bool areSameSize = (this._myFilterHashes.Length == type.myTypeHashes.Length);

                for (int i = 0; i < this._myFilterHashes.Length; i++)
                {
                    if (this._myFilterHashes[i] == MATCHANY)
                    {
                        break;
                    }

                    if (i < type.myTypeHashes.Length)
                    {
                        // The current filter does not match exit
                        // otherwise we compare the next round.
                        if (this._myFilterHashes[i] != type.myTypeHashes[i])
                        {
                            lret = false;
                            break;
                        }

                        // We are still here when the last arry item matches
                        // This is a full match
                        if (i == this._myFilterHashes.Length - 1 && areSameSize)
                        {
                            break;
                        }
                    }
                    else // the filter string is longer than the domain. That can never match
                    {
                        lret = false;
                        break;
                    }
                }
            }

            if (lret)
            {
                lret = (msgTypeFilter & this.myMsgTypeFilter) != MessageTypes.None;
            }

            // If no match try next filter
            if (this.Next != null && lret == false)
            {
                lret = this.Next.IsMatch(type, msgTypeFilter, level);
            }

            return lret;
        }
    }
}
