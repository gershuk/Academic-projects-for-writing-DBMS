using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class ColumnItemNode : SqlNode
    {
        public List<string> Id { get; set; }

        public override void CollectInfoFromChild() => Id = FindFirstChildNodeByType<ColumnSourceNode>().Id;
    }
}
