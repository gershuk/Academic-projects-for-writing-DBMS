using System.Collections.Generic;
using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class IdOperatorNode : SqlCommandNode
    {
        public override List<TableLock> GetCommandInfo() => new List<TableLock>();
    }
}
