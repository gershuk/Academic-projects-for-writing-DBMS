using System.Collections.Generic;
using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class TransactionBeginOptNode : SqlNode
    {
        public Id TransactionName { get; set; }

        public override void CollectInfoFromChild ()
        {
            var idNode = FindFirstChildNodeByType<TransactionNameNode>();
            TransactionName =new Id(idNode?.TransactionName);
        }
    }
}
