﻿namespace BackwardCompatibilityChecker.Infrastructure.Diagnostics
{
    class TraceFilterMatchAll : TraceFilter
    {
        public override bool IsMatch(TypeHashes type, MessageTypes msgTypeFilter, Level level)
        {
            return true;    
        }
    }
}
