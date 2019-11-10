using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    internal class FromClauseNode : SqlNode
    {
        public List<string> IdList { get; set; }

        public override void CollectInfoFromChild()
        {
            var idLinkNode = FindFirstChildNodeByType<IdLinkNode>();
            IdList = idLinkNode?.TableName;
        }
    }
}
