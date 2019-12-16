namespace IronySqlParser.AstNodes
{
    public class StringLiteralNode : SqlNode
    {
        public string StringLiteral { get; set; }

        public override void CollectDataFromChildren ()
        {
            foreach (var token in Tokens)
            {
                StringLiteral = (string)token.Value;
            }
        }
    }
}
