using System.Collections.Generic;
using System.Linq;

namespace IronySqlParser.AstNodes
{
    internal class JoinChainOptNode : IdOperatorNode
    {
        public JoinKind JoinKind { get; set; }
        public List<string> LeftId { get; set; }
        public List<string> RightId { get; set; }
        public JoinStatementNode JoinStatementNode { get; set; }

        public override void CollectInfoFromChild()
        {
            var childNodes = ChildNodes.ToArray();
            JoinKind = FindFirstChildNodeByType<JoinKindOptNode>().JoinKindOpt;
            LeftId = (childNodes[0] as IdLinkNode).TableName;
            RightId = (childNodes[3] as IdLinkNode).TableName;
            JoinStatementNode = FindFirstChildNodeByType<JoinStatementNode>();
        }
    }
}
