using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class StmtListNode : SqlNode
    {
        public List<SqlCommandNode> SqlCommands { get; set; }

        public override void CollectInfoFromChild() => SqlCommands = FindAllChildNodesByType<SqlCommandNode>();
    }
}
