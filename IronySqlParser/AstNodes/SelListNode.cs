using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class SelListNode : SqlNode
    {
        public List<List<string>> IdList { get; set; }

        public override void CollectInfoFromChild() => IdList = FindFirstChildNodeByType<ColumnItemListNode>()?.IdList;
    }
}
