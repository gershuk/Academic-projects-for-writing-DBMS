using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronySqlParser.AstNodes
{
    class ColumnSourceNode:SqlNode
    {
        public string Id { get; set; }

        public override void CollectInfoFromChild()
        {
            Id = FindChildNodesByType<IdNode>()[0].Id;
        }
    }
}
