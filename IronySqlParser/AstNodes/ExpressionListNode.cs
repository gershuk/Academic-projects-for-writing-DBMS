using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    internal class ExpressionListNode : SqlNode
    {
        public List<ExpressionNode> Expressions { get; set; }

        public override void CollectInfoFromChild() => Expressions = FindAllChildNodesByType<ExpressionNode>();
    }
}
