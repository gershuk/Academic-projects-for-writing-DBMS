using System.Collections.Generic;

using DataBaseType;

using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class UpdateCommandNode : SqlCommandNode
    {
        public Id TableName { get; set; }
        public List<AssignmentNode> Assignments { get; set; }
        public ExpressionNode WhereExpression { get; set; }

        public override void CollectDataFromChildren ()
        {
            TableName = FindFirstChildNodeByType<IdNode>()?.Id;
            Assignments = FindFirstChildNodeByType<AssignmentListNode>()?.Assignments;
            WhereExpression = FindFirstChildNodeByType<WhereClauseNode>()?.Expression;
        }

        public override List<TableLock> GetTableLocks () => new List<TableLock>() { new TableLock(LockType.Update, TableName.ToString(), new System.Threading.ManualResetEvent(false)) };
    }
}
