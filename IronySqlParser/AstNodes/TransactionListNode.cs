using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    internal class TransactionListNode : SqlNode
    {
        private List<TransactionNode> TransactionNodes { get; set; }

        public override void CollectInfoFromChild() => TransactionNodes = FindAllChildNodesByType<TransactionNode>();
    }
}
