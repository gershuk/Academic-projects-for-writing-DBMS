using System.Collections.Generic;

using DataBaseType;

using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class DropTableCommandNode : SqlCommandNode
    {
        public Id TableName { get; set; }

        public override void CollectDataFromChildren () => TableName = new Id(FindFirstChildNodeByType<IdNode>().Id);

        public override List<TableLock> GetTableLocks () => new List<TableLock>();
    }
}
