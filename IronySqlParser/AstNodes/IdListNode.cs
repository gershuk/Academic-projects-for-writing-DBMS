using System.Collections.Generic;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class IdListNode : SqlNode
    {
        public List<Id> IdList { get; set; }

        public override void CollectDataFromChildren ()
        {
            var idListNode = FindAllChildNodesByType<IdNode>();

            IdList = new List<Id>();

            foreach (var id in idListNode)
            {
                IdList.Add(id.Id);
            }
        }
    }
}
