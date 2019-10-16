using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class FieldDefListNode : SqlNode
    {
        public List<FieldDefNode> FieldDefList { get; set; }

        public override void CollectInfoFromChild() => FieldDefList = FindChildNodesByType<FieldDefNode>();
    }
}
