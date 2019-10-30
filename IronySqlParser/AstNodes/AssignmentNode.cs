using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class AssignmentNode : SqlNode
    {
        public List<string> Id { get; set; }
        public ExpressionNode Expression { get; set; }

        public override void CollectInfoFromChild()
        {
            Id = FindChildNodesByType<IdNode>()[0].Id;
            Expression = FindChildNodesByType<ExpressionNode>()[0];
        }
    }
}
