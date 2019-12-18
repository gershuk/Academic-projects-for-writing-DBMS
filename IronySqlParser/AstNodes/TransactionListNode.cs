using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class TransactionListNode : SqlNode
    {
        public List<TransactionNode> TransactionNodes { get; set; }

        public override void CollectDataFromChildren () => TransactionNodes = FindAllChildNodesByType<TransactionNode>();
    }
}
