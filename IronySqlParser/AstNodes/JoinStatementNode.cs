using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronySqlParser.AstNodes
{
    class JoinStatementNode:SqlNode
    {
        public List<string> LeftId { get; set; }  
        public List<string> RightId { get; set; }

        public override void CollectInfoFromChild()
        {
            var childNodes = ChildNodes.ToArray();
            LeftId =  (childNodes[0] as IdNode).Id;
            RightId = (childNodes[2] as IdNode).Id;
        }
    }
}
