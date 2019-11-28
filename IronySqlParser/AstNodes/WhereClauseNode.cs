namespace IronySqlParser.AstNodes
{
    public class WhereClauseNode : SqlNode
    {
        public ExpressionNode Expression { get; set; }

        public override void CollectInfoFromChild() => Expression = FindFirstChildNodeByType<ExpressionNode>();
    }
}

