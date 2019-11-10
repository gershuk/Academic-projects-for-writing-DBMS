using System.Collections.Generic;
using System.Linq;

namespace IronySqlParser.AstNodes
{
    internal class IntersectChainOptNode : IdOperatorNode
    {
        public List<string> LeftId { get; set; }
        public List<string> RightId { get; set; }

        public override void CollectInfoFromChild()
        {
            var childNodes = ChildNodes.ToArray();
            LeftId = (childNodes[0] as IdLinkNode).TableName;
            RightId = (childNodes[2] as IdLinkNode).TableName;
        }
    }
}
