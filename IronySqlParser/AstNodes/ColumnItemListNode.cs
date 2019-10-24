using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class ColumnItemListNode : SqlNode
    {
        public List<string> IdList { get; set; }

        public override void CollectInfoFromChild()
        {
            var columnItemNodes = FindChildNodesByType<ColumnItemNode>();

            IdList = new List<string>();

            foreach (var columnItemNode in columnItemNodes)
            {
                IdList.Add(columnItemNode.Id);
            }
        }
    }
}
