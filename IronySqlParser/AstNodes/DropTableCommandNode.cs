using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class DropTableCommandNode : SqlCommandNode
    {
        public List<string> TableName { get; set; }

        public override void CollectInfoFromChild() => TableName = FindFirstChildNodeByType<IdNode>().Id;
    }
}
