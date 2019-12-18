using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class TransactionNameNode : SqlNode
    {
        public List<string> TransactionName { get; set; }

        public override void CollectDataFromChildren ()
        {
            var idNode = FindFirstChildNodeByType<IdNode>();
            TransactionName = idNode?.Id;
        }
    }
}
