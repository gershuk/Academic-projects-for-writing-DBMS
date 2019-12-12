using System.Collections.Generic;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class IdListNode : SqlNode
    {
        public List<Id> IdList { get; set; }

        public override void CollectInfoFromChild ()
        {
            var idListNode = FindAllChildNodesByType<IdNode>();

            IdList = new List<Id>();

            foreach (var id in idListNode)
            {
                IdList.Add(new Id(id.Id));
            }
        }
    }
}
