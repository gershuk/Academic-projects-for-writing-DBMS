using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class FieldDefListNode : SqlNode
    {
        public List<FieldDefNode> FieldDefList { get; set; }

        public override void CollectInfoFromChild () => FieldDefList = FindAllChildNodesByType<FieldDefNode>();
    }
}
