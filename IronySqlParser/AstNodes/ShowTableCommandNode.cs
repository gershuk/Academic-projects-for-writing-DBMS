using System.Collections.Generic;
using DataBaseType;
using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class ShowTableCommandNode : SqlCommandNode
    {
        public Id TableName { get; set; }

        public override void CollectInfoFromChild () => TableName = new Id(FindFirstChildNodeByType<IdNode>()?.Id);
        public override List<TableLock> GetTableLocks () => new List<TableLock>();
    }
}
