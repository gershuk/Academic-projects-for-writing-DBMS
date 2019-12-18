using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class ExpressionListNode : SqlNode
    {
        public List<ExpressionNode> Expressions { get; set; }

        public override void CollectDataFromChildren () => Expressions = FindAllChildNodesByType<ExpressionNode>();
    }
}
