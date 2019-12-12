using System.Collections.Generic;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class SelListNode : SqlNode
    {
        public List<Id> IdList { get; set; }

        public override void CollectInfoFromChild () => IdList = FindFirstChildNodeByType<ColumnItemListNode>()?.IdList ??
                new List<Id>() { (new Id(new List<string>() { "*" })) };
    }
}
