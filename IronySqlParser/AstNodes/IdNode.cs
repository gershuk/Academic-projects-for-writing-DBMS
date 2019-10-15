using System.Linq;

using IronySqlParser.AstNodes;

namespace IronySqlParser
{
    class IdNode : SimpleIdNode
    {
        public string Id { get; set; }

        public override void CollectInfoFromChild()
        {
            var simpleIdList = FindChildNodesByType<SimpleIdNode>();

            foreach (var simpleId in simpleIdList)
            {
                Id += simpleId.Tokens.First<Token>().Text + " ";
            }

            Id = Id.TrimEnd(' ');
        }
    }
}
