using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class TransactionBeginOptNode : SqlNode
    {
        public List<string> TransactionName { get; set; }

        public override void CollectInfoFromChild()
        {
            var idNode = FindFirstChildNodeByType<TransactionNameNode>();
            TransactionName = idNode?.TransactionName;
        }
    }
}
