using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class FieldDefListNode : SqlNode
    {
        public List<FieldDefNode> FieldDefList { get; set; }

        public override void CollectDataFromChildren () => FieldDefList = FindAllChildNodesByType<FieldDefNode>();
    }
}
