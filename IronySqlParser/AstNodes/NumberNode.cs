namespace IronySqlParser.AstNodes
{
    public enum NumberType
    {
        Double,
        Int
    }

    public class NumberNode : SqlNode
    {
        public NumberType NumberType { get; set; }
        public double NumberDouble { get; set; }
        public int NumberInt { get; set; }

        public override void CollectInfoFromChild()
        {
            Token numberToken = default;

            foreach (var token in Tokens)
            {
                numberToken = token;
            }

            switch (numberToken.Value)
            {
                case int number:
                    NumberInt = number;
                    NumberType = NumberType.Int;
                    break;
                case double number:
                    NumberDouble = number;
                    NumberType = NumberType.Double;
                    break;
            }
        }
    }
}
