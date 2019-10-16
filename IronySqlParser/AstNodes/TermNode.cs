using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronySqlParser.AstNodes
{
    public enum TermType
    {
        Id,
        StringLiteral,
        NumberDouble,
        NumberInt
    }

    class TermNode : SqlNode
    {
        public TermType TermType { get; set; }

        public string Id { get; set; }
        public string StringLiteral { get; set; }
        public double NumberDouble { get; set; }
        public int NumberInt { get; set; }

        public override void CollectInfoFromChild()
        {
            var IdNode = FindChildNodesByType<IdNode>();
            var StringLiteralNode = FindChildNodesByType<StringLiteralNode>();
            var NumberNode = FindChildNodesByType<NumberNode>();

            if (IdNode.Count > 0)
            {
                TermType = TermType.Id;
                Id = IdNode[0].Id;
            }

            if (StringLiteralNode.Count > 0)
            {
                TermType = TermType.StringLiteral;
                StringLiteral = StringLiteralNode[0].StringLiteral;
            }

            if (NumberNode.Count > 0)
            {
                switch (NumberNode[0].NumberType)
                {
                    case NumberType.Double:
                        NumberDouble = NumberNode[0].NumberDouble;
                        TermType = TermType.NumberDouble;
                        break;
                    case NumberType.Int:
                        NumberInt = NumberNode[0].NumberInt;
                        TermType = TermType.NumberInt;
                        break;
                }
            }
        }
    }
}
