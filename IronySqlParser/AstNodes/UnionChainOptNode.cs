using System.Collections.Generic;
using System.Linq;

using DataBaseType;

using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class UnionChainOptNode : IdOperatorNode
    {
        public UnionKind UnionKind { get; set; }
        public List<string> LeftId { get; set; }
        public List<string> RightId { get; set; }

        public override void CollectInfoFromChild()
        {
            var childNodes = ChildNodes.ToArray();
            UnionKind = FindFirstChildNodeByType<UnionKindOptNode>().UnionKindOpt;
            LeftId = (childNodes[0] as IdLinkNode).TableName;
            RightId = (childNodes[3] as IdLinkNode).TableName;
        }

        public override List<TableLock> GetTableLocks() => new List<TableLock>() { new TableLock(LockType.Read, LeftId, new System.Threading.ManualResetEvent(false)),
            new TableLock(LockType.Read, RightId, new System.Threading.ManualResetEvent(false)) };
    }
}
