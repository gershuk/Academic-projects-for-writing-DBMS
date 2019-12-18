using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class InsertDataListNode : SqlNode
    {
        public List<InsertObjectNode> InsertObjects { get; set; }

        public override void CollectDataFromChildren () => InsertObjects = FindAllChildNodesByType<InsertObjectNode>();
    }
}
