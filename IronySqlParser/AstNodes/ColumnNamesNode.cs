namespace IronySqlParser.AstNodes
{
    class ColumnNamesNode : SqlNode
    {
        public IdListNode IdListNode { get; set; }
        public override void CollectInfoFromChild() => IdListNode = FindChildNodesByType<IdListNode>()[0];
    }
}
