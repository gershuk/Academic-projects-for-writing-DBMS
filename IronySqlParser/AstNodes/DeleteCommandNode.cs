using System.Collections.Generic;
using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class DeleteCommandNode : SqlCommandNode
    {
        public List<string> TableName { get; set; }
        public WhereClauseNode WhereClauseNode { get; set; }

        public override void CollectInfoFromChild()
        {
            TableName = FindFirstChildNodeByType<IdNode>().Id;
            WhereClauseNode = FindFirstChildNodeByType<WhereClauseNode>();
        }

        public override List<TableLock> GetCommandInfo() => new List<TableLock>() { new TableLock(LockType.Update, TableName, new System.Threading.ManualResetEvent(false)) };
    }
}
