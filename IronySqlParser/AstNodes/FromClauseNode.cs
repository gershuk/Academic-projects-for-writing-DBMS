using System.Collections.Generic;
using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class FromClauseNode : SqlNode
    {
        public Id IdList { get; set; }

        public override void CollectInfoFromChild ()
        {
            var idLinkNode = FindFirstChildNodeByType<IdLinkNode>();
            IdList = idLinkNode?.TableName;
        }
    }
}
