namespace IronySqlParser.AstNodes
{
    class InsertDataNode : SqlNode
    {
        public InsertDataListNode InsertDataListNode { get; set; }

        public override void CollectInfoFromChild() => InsertDataListNode = FindChildNodesByType<InsertDataListNode>()[0];
    }
}
