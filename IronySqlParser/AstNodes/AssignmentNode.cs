using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class AssignmentNode : SqlNode
    {
        public Id Id { get; set; }
        public ExpressionNode Expression { get; set; }

        public override void CollectDataFromChildren ()
        {
            Id = FindFirstChildNodeByType<IdNode>().Id;
            Expression = FindFirstChildNodeByType<ExpressionNode>();
        }
    }
}
