using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class TransactionBeginOptNode : SqlNode
    {
        public Id TransactionName { get; set; }

        public override void CollectDataFromChildren ()
        {
            var idNode = FindFirstChildNodeByType<TransactionNameNode>();
            TransactionName = idNode?.TransactionName;
        }
    }
}
