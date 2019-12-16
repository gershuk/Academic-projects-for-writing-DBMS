namespace IronySqlParser.AstNodes
{
    public class JoinStatementNode : SqlNode
    {
        public ExpressionNode ExpressionNode { get; set; }

        public override void CollectDataFromChildren () => ExpressionNode = FindFirstChildNodeByType<ExpressionNode>();

    }
}
