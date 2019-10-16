using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronySqlParser.AstNodes
{
    class ColumnItemNode:SqlNode
    {
        public string Id { get; set; }

        public override void CollectInfoFromChild()
        {
            Id = FindChildNodesByType<ColumnSourceNode>()[0].Id;
        }
    }
}
