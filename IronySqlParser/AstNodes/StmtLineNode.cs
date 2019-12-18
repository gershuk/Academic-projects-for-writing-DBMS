namespace IronySqlParser.AstNodes
{
    public class StmtLineNode : SqlNode
    {
        public SqlCommandNode SqlCommand { get; set; }
        public override void CollectDataFromChildren () => SqlCommand = FindFirstChildNodeByType<SqlCommandNode>();
    }
}
