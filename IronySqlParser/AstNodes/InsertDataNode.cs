namespace IronySqlParser.AstNodes
{
    public class InsertDataNode : SqlNode
    {
        public InsertDataListNode InsertDataListNode { get; set; }

        public override void CollectDataFromChildren () => InsertDataListNode = FindFirstChildNodeByType<InsertDataListNode>();
    }
}
