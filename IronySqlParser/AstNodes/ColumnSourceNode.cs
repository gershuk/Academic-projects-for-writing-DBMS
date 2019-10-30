using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class ColumnSourceNode : SqlNode
    {
        public List<string> Id { get; set; }

        public override void CollectInfoFromChild() => Id = FindChildNodesByType<IdNode>()[0].Id;
    }
}
