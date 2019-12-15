using System;

namespace IronySqlParser.AstNodes
{
    public class BetweenTimeSelectorNode : TimeSelectorNode
    {
        public override void IsTimeValide (DateTime time) => throw new NotImplementedException();
    }
}
