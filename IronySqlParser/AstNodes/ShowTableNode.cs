namespace IronySqlParser.AstNodes
{
    class ShowTableNode : SqlNode
    {
        public string TableName { get; set; }

        public override void CollectInfoFromChild()
        {
            TableName = FindChildNodesByType<IdNode>()[0].Id;
        }
    }
}
