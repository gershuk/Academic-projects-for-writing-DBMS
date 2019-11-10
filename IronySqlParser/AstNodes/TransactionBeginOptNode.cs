using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    internal class TransactionBeginOptNode : SqlNode
    {
        public List<string> TransactionName { get; set; }

        public override void CollectInfoFromChild()
        {
            var idNode = FindFirstChildNodeByType<TransactionNameNode>();
            TransactionName = idNode?.TransactionName;
        }
    }
}
