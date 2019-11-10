using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    internal class AssignmentNode : SqlNode
    {
        public List<string> Id { get; set; }
        public ExpressionNode Expression { get; set; }

        public override void CollectInfoFromChild()
        {
            Id = FindFirstChildNodeByType<IdNode>()?.Id;
            Expression = FindFirstChildNodeByType<ExpressionNode>();
        }
    }
}
