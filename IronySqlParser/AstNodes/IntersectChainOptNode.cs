using System.Collections.Generic;
using System.Linq;
using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class IntersectChainOptNode : IdOperatorNode
    {
        public List<string> LeftId { get; set; }
        public List<string> RightId { get; set; }

        public override void CollectInfoFromChild()
        {
            var childNodes = ChildNodes.ToArray();
            LeftId = (childNodes[0] as IdLinkNode).TableName;
            RightId = (childNodes[2] as IdLinkNode).TableName;
        }

        public override List<TableLock> GetTableLocks() => new List<TableLock>() { new TableLock(LockType.Read, LeftId, new System.Threading.ManualResetEvent(false)),
            new TableLock(LockType.Read, RightId, new System.Threading.ManualResetEvent(false)) };
    }
}
