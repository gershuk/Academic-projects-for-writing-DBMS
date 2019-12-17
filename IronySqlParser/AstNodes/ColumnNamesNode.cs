namespace IronySqlParser.AstNodes
{
    public class ColumnNamesNode : SqlNode
    {
        public IdListNode IdListNode { get; set; }
        public override void CollectDataFromChildren () => IdListNode = FindFirstChildNodeByType<IdListNode>();
    }
}
