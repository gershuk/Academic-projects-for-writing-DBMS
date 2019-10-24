namespace IronySqlParser.AstNodes
{
    class ColumnItemNode : SqlNode
    {
        public string Id { get; set; }

        public override void CollectInfoFromChild() => Id = FindChildNodesByType<ColumnSourceNode>()[0].Id;
    }
}
