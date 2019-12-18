using System;

namespace IronySqlParser.AstNodes
{
    public class AsOfTimeSelectorNode : TimeSelectorNode
    {
        private DateTime _dateTime;

        public override void CollectDataFromChildren ()=> _dateTime = FindFirstChildNodeByType<DateTimeNode>().DateTime;

        public override bool IsTimeValide (DateTime startTime, DateTime endTime) => startTime <= _dateTime && _dateTime < endTime;
    }
}
