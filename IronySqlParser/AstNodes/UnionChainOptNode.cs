using System.Collections.Generic;
using System.Linq;

namespace IronySqlParser.AstNodes
{
    internal class UnionChainOptNode : IdOperatorNode
    {
        public UnionKind UnionKind { get; set; }
        public List<string> LeftId { get; set; }
        public List<string> RightId { get; set; }

        public override void CollectInfoFromChild()
        {
            var childNodes = ChildNodes.ToArray();
            UnionKind = FindFirstChildNodeByType<UnionKindOptNode>().UnionKindOpt;
            LeftId = (childNodes[0] as IdLinkNode).TableName;
            RightId = (childNodes[3] as IdLinkNode).TableName;
        }
    }
}
