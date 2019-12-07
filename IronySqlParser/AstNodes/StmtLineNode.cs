namespace IronySqlParser.AstNodes
{
    public class StmtLineNode : SqlNode
    {
        public SqlCommandNode SqlCommand { get; set; }
        public override void CollectInfoFromChild () => SqlCommand = FindFirstChildNodeByType<SqlCommandNode>();
    }
}
