using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class IdListNode : SqlNode
    {
        public List<List<string>> IdList { get; set; }

        public override void CollectInfoFromChild()
        {
            var idListNode = FindChildNodesByType<IdNode>();

            IdList = new List<List<string>>();

            foreach (var id in idListNode)
            {
                IdList.Add(id.Id);
            }
        }
    }
}
