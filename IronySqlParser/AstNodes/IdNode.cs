using System.Collections.Generic;
using System.Linq;

using IronySqlParser.AstNodes;

namespace IronySqlParser
{
    class IdNode : SimpleIdNode
    {
        public List<string> Id { get; set; }

        public override void CollectInfoFromChild()
        {
            var simpleIdList = FindChildNodesByType<SimpleIdNode>();

            foreach (var simpleId in simpleIdList)
            {
                Id.Add(simpleId.Tokens.First<Token>().Text);
            }
        }
    }
}
