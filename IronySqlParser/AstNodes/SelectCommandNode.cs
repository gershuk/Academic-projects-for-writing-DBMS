using System.Collections.Generic;
using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class SelectCommandNode : SqlCommandNode
    {

        public List<List<string>> ColumnIdList { get; set; }
        public List<string> TableName { get; set; }
        public ExpressionNode WhereExpression { get; set; }

        public override void CollectInfoFromChild()
        {
            ColumnIdList = FindFirstChildNodeByType<SelListNode>()?.IdList;
            TableName = FindFirstChildNodeByType<FromClauseNode>()?.IdList;
            WhereExpression = FindFirstChildNodeByType<WhereClauseNode>()?.Expression;
        }

        public override List<TableLock> GetCommandInfo() => new List<TableLock>() { new TableLock(LockType.Read, TableName, new System.Threading.ManualResetEvent(false)) };
    }
}

