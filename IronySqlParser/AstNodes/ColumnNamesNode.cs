namespace IronySqlParser.AstNodes
{
    internal class ColumnNamesNode : SqlNode
    {
        public IdListNode IdListNode { get; set; }
        public override void CollectInfoFromChild() => IdListNode = FindFirstChildNodeByType<IdListNode>();
    }
}
