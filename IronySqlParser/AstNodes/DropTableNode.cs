namespace IronySqlParser.AstNodes
{
    public class DropTableNode : SqlNode
    {
        public string TableName { get; set; }

        public override void CollectInfoFromChild() => TableName = FindChildNodesByType<IdNode>()[0].Id;
    }
}
