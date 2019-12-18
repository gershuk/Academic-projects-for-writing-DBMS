using System.Collections.Generic;
using System.Linq;

using DataBaseType;

using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class IntersectChainOptNode : IdOperatorNode
    {
        public Id LeftId { get; set; }
        public Id RightId { get; set; }

        public override void CollectDataFromChildren ()
        {
            var childNodes = ChildNodes.ToArray();
            LeftId = (childNodes[0] as IdLinkNode).TableName;
            RightId = (childNodes[2] as IdLinkNode).TableName;
        }

        public override List<TableLock> GetTableLocks () => new List<TableLock>() { new TableLock(LockType.Read, LeftId.SimpleIds, new System.Threading.ManualResetEvent(false)),
            new TableLock(LockType.Read, RightId.SimpleIds, new System.Threading.ManualResetEvent(false)) };
    }
}
