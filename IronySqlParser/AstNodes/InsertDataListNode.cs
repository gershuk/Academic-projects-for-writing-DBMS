using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class InsertDataListNode : SqlNode
    {
        public List<InsertObjectNode> InsertObjects { get; set; }

        public override void CollectInfoFromChild() => InsertObjects = FindAllChildNodesByType<InsertObjectNode>();
    }
}
