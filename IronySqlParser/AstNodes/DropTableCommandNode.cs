using System.Collections.Generic;
using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class DropTableCommandNode : SqlCommandNode
    {
        public List<string> TableName { get; set; }

        public override void CollectInfoFromChild () => TableName = FindFirstChildNodeByType<IdNode>().Id;

        public override List<TableLock> GetTableLocks () => new List<TableLock>();
    }
}
