namespace BackwardCompatibilityChecker.Infrastructure.Diagnostics
{
    class TraceFilterMatchNone : TraceFilter
    {
        public TraceFilterMatchNone()
        {
        }

        public override bool IsMatch(TypeHashes type, MessageTypes msgTypeFilter, Level level)
        {
            return false;
        }
    }
}
