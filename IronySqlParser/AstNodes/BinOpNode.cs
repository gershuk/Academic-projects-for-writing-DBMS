using System.Linq;

namespace IronySqlParser.AstNodes
{
    public enum BinOp
    {
        Plus,
        Minus,
        Multiply,
        Divide,
        Modulo,
        BitAnd,
        BitOr,
        ExclusiveOr,
        Equal,
        Greater,
        Less,
        EqualGreater,
        EqualLess,
        NotEqual,
        And,
        Or
    }

    class BinOpNode : SqlNode
    {
        public BinOp BinOp { get; set; }

        public override void CollectInfoFromChild()
        {
            switch ((ChildNodes.First<ISqlNode>() as SqlKeyNode).Text.ToUpper())
            {
                case "+":
                    BinOp = BinOp.Plus;
                    break;
                case "-":
                    BinOp = BinOp.Minus;
                    break;
                case "*":
                    BinOp = BinOp.Multiply;
                    break;
                case "/":
                    BinOp = BinOp.Divide;
                    break;
                case "%":
                    BinOp = BinOp.Modulo;
                    break;
                case "&":
                    BinOp = BinOp.BitAnd;
                    break;
                case "|":
                    BinOp = BinOp.BitOr;
                    break;
                case "^":
                    BinOp = BinOp.ExclusiveOr;
                    break;
                case "=":
                    BinOp = BinOp.Equal;
                    break;
                case ">":
                    BinOp = BinOp.Greater;
                    break;
                case "<":
                    BinOp = BinOp.Less;
                    break;
                case ">=":
                    BinOp = BinOp.EqualGreater;
                    break;
                case "<=":
                    BinOp = BinOp.EqualLess;
                    break;
                case "!=":
                    BinOp = BinOp.NotEqual;
                    break;
                case "AND":
                    BinOp = BinOp.And;
                    break;
                case "OR":
                    BinOp = BinOp.Or;
                    break;
            }
        }
    }
}
