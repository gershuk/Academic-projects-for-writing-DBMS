using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class ColumnSourceNode : SqlNode
    {
        public List<string> Id { get; set; }

        public override void CollectDataFromChildren () => Id = FindFirstChildNodeByType<IdNode>().Id;
    }
}
