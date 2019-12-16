using System;

namespace IronySqlParser.AstNodes
{
    public class FromToTimeSelectorNode : TimeSelectorNode
    {
        private DateTime _startTime;
        private DateTime _endTime;

        public override void CollectDataFromChildren ()
        {
            var dateTimeNodes = FindAllChildNodesByType<DateTimeNode>();
            _startTime = dateTimeNodes[0].DateTime;
            _endTime = dateTimeNodes[1].DateTime;
        }

        public override bool IsTimeValide (DateTime startTime, DateTime endTime) => startTime < _endTime && endTime > _startTime && _startTime<=_endTime;
    }
}
