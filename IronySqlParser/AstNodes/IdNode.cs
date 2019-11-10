using System.Collections.Generic;
using System.Linq;

using IronySqlParser.AstNodes;

namespace IronySqlParser
{
    internal class IdNode : SimpleIdNode
    {
        public List<string> Id { get; private set; } = new List<string>();

        public override void CollectInfoFromChild()
        {
            var simpleIdList = FindAllChildNodesByType<SimpleIdNode>();

            foreach (var simpleId in simpleIdList)
            {
                Id.Add(simpleId.Tokens.First<Token>().Text);
            }
        }
    }
}
