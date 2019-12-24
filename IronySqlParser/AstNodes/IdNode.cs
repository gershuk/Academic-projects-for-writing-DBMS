using System.Collections.Generic;
using System.Linq;
using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class IdNode : SimpleIdNode
    {
        public Id Id { get; private set; }

        public override void CollectDataFromChildren ()
        {
            var simpleId = FindAllChildNodesByType<SimpleIdNode>();
            var simpleIdList = new List<string>();

            foreach (var id in simpleId)
            {
                simpleIdList.Add(id.Tokens.First<Token>().Text);
            }

            Id = new Id(simpleIdList);
        }
    }
}
