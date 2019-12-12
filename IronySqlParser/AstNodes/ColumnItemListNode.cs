using System.Collections.Generic;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class ColumnItemListNode : SqlNode
    {
        public List<Id> IdList { get; set; }

        public override void CollectInfoFromChild ()
        {
            var columnItemNodes = FindAllChildNodesByType<ColumnItemNode>();

            IdList = new List<Id>();

            foreach (var columnItemNode in columnItemNodes)
            {
                IdList.Add(columnItemNode.Id);
            }
        }
    }
}
