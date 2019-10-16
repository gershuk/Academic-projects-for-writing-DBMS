using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronySqlParser.AstNodes
{
    class ColumnItemListNode:SqlNode
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
