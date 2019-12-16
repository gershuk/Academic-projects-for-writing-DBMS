using System.Collections.Generic;
using System.Linq;

namespace IronySqlParser.AstNodes
{
    public class IdNode : SimpleIdNode
    {
        public List<string> Id { get; private set; } = new List<string>();

        public override void CollectDataFromChildren ()
        {
            var simpleIdList = FindAllChildNodesByType<SimpleIdNode>();

            foreach (var simpleId in simpleIdList)
            {
                Id.Add(simpleId.Tokens.First<Token>().Text);
            }
        }
    }
}
