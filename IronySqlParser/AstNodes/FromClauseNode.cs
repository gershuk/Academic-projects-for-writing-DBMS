using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronySqlParser.AstNodes
{
    class FromClauseNode : SqlNode
    {
        public List<string> IdList { get; set; }

        public override void CollectInfoFromChild()
        {
            IdList = FindChildNodesByType<IdListNode>()[0].IdList;
        }
    }
}
