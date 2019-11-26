using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    internal class ColumnItemNode : SqlNode
    {
        public List<string> Id { get; set; }

        public override void CollectInfoFromChild() => Id = FindFirstChildNodeByType<ColumnSourceNode>().Id;
    }
}
