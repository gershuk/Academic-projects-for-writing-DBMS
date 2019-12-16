using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class InsertObjectNode : SqlNode
    {
        public List<ExpressionNode> ObjectParams { get; set; }

        public override void CollectDataFromChildren () => ObjectParams = FindFirstChildNodeByType<ExpressionListNode>()?.Expressions;
    }
}
