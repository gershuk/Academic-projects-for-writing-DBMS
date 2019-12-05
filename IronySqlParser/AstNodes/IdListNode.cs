using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class IdListNode : SqlNode
    {
        public List<List<string>> IdList { get; set; }

        public override void CollectInfoFromChild ()
        {
            var idListNode = FindAllChildNodesByType<IdNode>();

            IdList = new List<List<string>>();

            foreach (var id in idListNode)
            {
                IdList.Add(id.Id);
            }
        }
    }
}
