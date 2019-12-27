using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class StringLiteralNode : SqlNode
    {
        public Varchar StringLiteral { get; set; }

        public override void CollectDataFromChildren ()
        {
            foreach (var token in Tokens)
            {
                StringLiteral = (Varchar)(string) token.Value;
            }
        }
    }
}
