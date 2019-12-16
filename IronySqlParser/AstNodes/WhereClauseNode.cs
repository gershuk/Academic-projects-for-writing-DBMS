namespace IronySqlParser.AstNodes
{
    public class WhereClauseNode : SqlNode
    {
        public ExpressionNode Expression { get; set; }

        public override void CollectDataFromChildren () => Expression = FindFirstChildNodeByType<ExpressionNode>();
    }
}

