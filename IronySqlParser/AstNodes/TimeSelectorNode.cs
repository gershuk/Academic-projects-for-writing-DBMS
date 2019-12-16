using System;

namespace IronySqlParser.AstNodes
{
    public abstract class TimeSelectorNode:SqlNode
    {
        public abstract bool IsTimeValide (DateTime startTime, DateTime endTime);
    }
}
