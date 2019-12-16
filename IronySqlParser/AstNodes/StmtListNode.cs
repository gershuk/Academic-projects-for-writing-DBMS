using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class StmtListNode : SqlNode
    {
        public List<StmtLineNode> StmtList { get; set; }

        public override void CollectDataFromChildren () => StmtList = FindAllChildNodesByType<StmtLineNode>();
    }
}
