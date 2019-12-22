using System.Collections.Generic;
using System.Linq;

using DataBaseType;

using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class JoinChainOptNode : IdOperatorNode
    {
        public JoinKind JoinKind { get; set; }
        public Id LeftId { get; set; }
        public Id RightId { get; set; }
        public JoinStatementNode JoinStatementNode { get; set; }

        public override void CollectDataFromChildren ()
        {
            var childNodes = ChildNodes.ToArray();
            JoinKind = FindFirstChildNodeByType<JoinKindOptNode>().JoinKindOpt;
            LeftId = (childNodes[0] as IdLinkNode).TableName;
            RightId = (childNodes[3] as IdLinkNode).TableName;
            JoinStatementNode = FindFirstChildNodeByType<JoinStatementNode>();
        }

        public override List<TableLock> GetTableLocks () => new List<TableLock>() { new TableLock(LockType.Read, LeftId.SimpleIds.ToString(), new System.Threading.ManualResetEvent(false)),
            new TableLock(LockType.Read, RightId.SimpleIds.ToString(), new System.Threading.ManualResetEvent(false)) };
    }
}
