using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronySqlParser.AstNodes
{
    class IdListNode:SqlNode
    {
        public List<string> IdList { get; set; }

        public override void CollectInfoFromChild()
        {
            var idListNode = FindChildNodesByType<IdNode>();

            IdList = new List<string>();

            foreach (var id in idListNode)
            {
                IdList.Add(id.Id);
            }
        }
    }
}
