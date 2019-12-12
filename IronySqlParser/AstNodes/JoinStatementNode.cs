using System.Linq;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class JoinStatementNode : SqlNode
    {
        public Id LeftId { get; set; }
        public Id RightId { get; set; }

        public override void CollectInfoFromChild ()
        {
            var childNodes = ChildNodes.ToArray();
            LeftId = new Id((childNodes[0] as IdNode).Id);
            RightId = new Id((childNodes[2] as IdNode).Id);
        }
    }
}
