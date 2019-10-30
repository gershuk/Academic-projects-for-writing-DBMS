namespace IronySqlParser.AstNodes
{
    class InsertNode : SqlNode
    {
        public IdNode TableName { get; set; }
        public ColumnNamesNode ColumnNames { get; set; }
        public InsertDataNode InsertDataNode { get; set; }

        public override void CollectInfoFromChild()
        {
            TableName = FindChildNodesByType<IdNode>()[0];
            ColumnNames = FindChildNodesByType<ColumnNamesNode>()[0];
            InsertDataNode = FindChildNodesByType<InsertDataNode>()[0];
        }
    }
}
