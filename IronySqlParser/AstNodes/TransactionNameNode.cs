using System.Collections.Generic;
using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class TransactionNameNode : SqlNode
    {
        public Id TransactionName { get; set; }

        public override void CollectDataFromChildren ()
        {
            var idNode = FindFirstChildNodeByType<IdNode>();
            TransactionName = idNode?.Id;
        }
    }
}
