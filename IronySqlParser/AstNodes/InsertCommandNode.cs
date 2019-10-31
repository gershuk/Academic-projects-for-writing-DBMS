namespace IronySqlParser.AstNodes
{
    class InsertCommandNode : SqlCommandNode
    {
        public IdNode TableName { get; set; }
        public ColumnNamesNode ColumnNames { get; set; }
        public InsertDataNode InsertDataNode { get; set; }

        public override void CollectInfoFromChild()
        {
            TableName = FindFirstChildNodeByType<IdNode>();
            ColumnNames = FindFirstChildNodeByType<ColumnNamesNode>();
            InsertDataNode = FindFirstChildNodeByType<InsertDataNode>();
        }
    }
}
