using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class SelListNode : SqlNode
    {
        public List<string> IdList { get; set; }

        public override void CollectInfoFromChild()
        {
            IdList = FindChildNodesByType<ColumnItemListNode>()[0].IdList;
        }
    }
}
