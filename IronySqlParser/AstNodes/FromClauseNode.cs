using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class FromClauseNode : SqlNode
    {
        public List<string> IdList { get; set; }

        public override void CollectInfoFromChild ()
        {
            var idLinkNode = FindFirstChildNodeByType<IdLinkNode>();
            IdList = idLinkNode?.TableName;
        }
    }
}
