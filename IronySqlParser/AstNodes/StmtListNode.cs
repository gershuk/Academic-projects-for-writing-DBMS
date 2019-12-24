using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class StmtListNode : SqlNode
    {
        public List<SqlCommandNode> StmtList { get; set; }

        public override void CollectDataFromChildren () => StmtList = FindAllChildNodesByType<SqlCommandNode>();
    }
}
