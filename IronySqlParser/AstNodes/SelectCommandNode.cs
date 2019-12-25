using System.Collections.Generic;

using DataBaseType;

using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class SelectCommandNode : SqlCommandNode
    {

        public List<Id> ColumnIdList { get; set; }
        public Id TableName { get; set; }
        public ExpressionNode WhereExpression { get; set; }
        public TimeSelectorNode TimeSelectorNode { get; set; }

        public override void CollectDataFromChildren ()
        {
            ColumnIdList = FindFirstChildNodeByType<SelListNode>()?.IdList;
            TableName = FindFirstChildNodeByType<FromClauseNode>()?.IdList;
            WhereExpression = FindFirstChildNodeByType<WhereClauseNode>()?.Expression;
            TimeSelectorNode = FindFirstChildNodeByType<ForClauseOptNode>()?.TimeSelectorNode;
        }

        public override List<TableLock> GetTableLocks () => new List<TableLock>() { new TableLock(LockType.Read, TableName.ToString(), new System.Threading.ManualResetEvent(false)) };
    }
}

