using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class ColumnItemNode : SqlNode
    {
        public Id Id { get; set; }

        public override void CollectInfoFromChild () => Id = new Id(FindFirstChildNodeByType<ColumnSourceNode>().Id);
    }
}
