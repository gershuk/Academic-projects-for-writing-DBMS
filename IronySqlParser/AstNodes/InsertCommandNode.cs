using System.Collections.Generic;

using DataBaseType;

using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class InsertCommandNode : SqlCommandNode
    {
        public Id TableName { get; set; }
        public ColumnNamesNode ColumnNames { get; set; }
        public InsertDataNode InsertDataNode { get; set; }

        public override void CollectDataFromChildren ()
        {
            TableName = FindFirstChildNodeByType<IdNode>().Id;
            ColumnNames = FindFirstChildNodeByType<ColumnNamesNode>();
            InsertDataNode = FindFirstChildNodeByType<InsertDataNode>();
        }

        public override List<TableLock> GetTableLocks () => new List<TableLock>() { new TableLock(LockType.Write, TableName.ToString(), new System.Threading.ManualResetEvent(false)) };
    }
}
