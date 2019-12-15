using System;

namespace IronySqlParser.AstNodes
{
    public class AsOfTimeSelectorNode : TimeSelectorNode
    {
        public override void IsTimeValide (DateTime time) => throw new NotImplementedException();
    }
}
