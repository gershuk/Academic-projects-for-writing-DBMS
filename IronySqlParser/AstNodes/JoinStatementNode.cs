using System.Collections.Generic;
using System.Linq;

namespace IronySqlParser.AstNodes
{
    public class JoinStatementNode : SqlNode
    {
        public List<string> LeftId { get; set; }
        public List<string> RightId { get; set; }

        public override void CollectInfoFromChild ()
        {
            var childNodes = ChildNodes.ToArray();
            LeftId = (childNodes[0] as IdNode).Id;
            RightId = (childNodes[2] as IdNode).Id;
        }
    }
}
