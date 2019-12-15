using System;

namespace IronySqlParser.AstNodes
{
    public abstract class TimeSelectorNode:SqlNode
    {
        public abstract void IsTimeValide (DateTime time);
    }
}
