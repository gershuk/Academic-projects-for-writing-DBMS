namespace IronySqlParser.AstNodes
{
    class WhereClauseNode : SqlNode
    {
        public ExpressionNode Expression { get; set; }

        public override void CollectInfoFromChild()
        {
            Expression = FindChildNodesByType<ExpressionNode>()[0];
        }
    }
}

