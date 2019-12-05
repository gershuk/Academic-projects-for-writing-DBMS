using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class StmtListNode : SqlNode
    {
        public List<StmtLineNode> StmtList { get; set; }

        public override void CollectInfoFromChild () => StmtList = FindAllChildNodesByType<StmtLineNode>();
    }
}
