using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class FromClauseNode : SqlNode
    {
        public List<List<string>> IdList { get; set; }

        public override void CollectInfoFromChild() => IdList = FindFirstChildNodeByType<IdListNode>()?.IdList;
    }
}
