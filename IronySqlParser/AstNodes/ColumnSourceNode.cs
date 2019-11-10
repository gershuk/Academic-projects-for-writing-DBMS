using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    internal class ColumnSourceNode : SqlNode
    {
        public List<string> Id { get; set; }

        public override void CollectInfoFromChild() => Id = FindFirstChildNodeByType<IdNode>().Id;
    }
}
