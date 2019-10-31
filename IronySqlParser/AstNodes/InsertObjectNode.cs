using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class InsertObjectNode : SqlNode
    {
        public List<ExpressionNode> ObjectParams { get; set; }

        public override void CollectInfoFromChild() => ObjectParams = FindFirstChildNodeByType<ExpressionListNode>()?.Expressions;
    }
}
