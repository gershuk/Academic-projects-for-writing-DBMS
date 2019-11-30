using System.Collections.Generic;
using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class UpdateCommandNode : SqlCommandNode
    {
        public List<string> TableName { get; set; }
        public List<AssignmentNode> Assignments { get; set; }
        public ExpressionNode WhereExpression { get; set; }

        public override void CollectInfoFromChild()
        {
            TableName = FindFirstChildNodeByType<IdNode>()?.Id;
            Assignments = FindFirstChildNodeByType<AssignmentListNode>()?.Assignments;
            WhereExpression = FindFirstChildNodeByType<WhereClauseNode>()?.Expression;
        }

        public override List<TableLock> GetTableLocks() => new List<TableLock>() { new TableLock(LockType.Update, TableName, new System.Threading.ManualResetEvent(false)) };
    }
}
