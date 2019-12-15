using System;

namespace IronySqlParser.AstNodes
{
    public class FromToTimeSelectorNode : TimeSelectorNode
    {
        public override void IsTimeValide (DateTime time) => throw new NotImplementedException();
    }
}
