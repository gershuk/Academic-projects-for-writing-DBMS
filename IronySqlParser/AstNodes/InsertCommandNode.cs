using System.Collections.Generic;
using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class InsertCommandNode : SqlCommandNode
    {
        public List<string> TableName { get; set; }
        public ColumnNamesNode ColumnNames { get; set; }
        public InsertDataNode InsertDataNode { get; set; }

        public override void CollectInfoFromChild()
        {
            TableName = FindFirstChildNodeByType<IdNode>().Id;
            ColumnNames = FindFirstChildNodeByType<ColumnNamesNode>();
            InsertDataNode = FindFirstChildNodeByType<InsertDataNode>();
        }

        public override List<TableLock> GetTableLocks() => new List<TableLock>() { new TableLock(LockType.Write, TableName, new System.Threading.ManualResetEvent(false)) };
    }
}
