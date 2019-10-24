using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class FromClauseNode : SqlNode
    {
        public List<string> IdList { get; set; }

        public override void CollectInfoFromChild() => IdList = FindChildNodesByType<IdListNode>()[0].IdList;
    }
}
