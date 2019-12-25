using System.Collections.Generic;
using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class ColumnSourceNode : SqlNode
    {
        public Id Id { get; set; }

        public override void CollectDataFromChildren () => Id = FindFirstChildNodeByType<IdNode>().Id;
    }
}
