namespace IronySqlParser.AstNodes
{
    public class InsertDataNode : SqlNode
    {
        public InsertDataListNode InsertDataListNode { get; set; }

        public override void CollectInfoFromChild() => InsertDataListNode = FindFirstChildNodeByType<InsertDataListNode>();
    }
}
