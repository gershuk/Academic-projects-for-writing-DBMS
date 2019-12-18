using System.Linq;

namespace IronySqlParser.AstNodes
{
    public enum UnOp
    {
        Plus,
        Minus,
        Not,
        Tilde
    }

    public class UnOpNode : SqlNode
    {
        public UnOp UnOp { get; set; }

        public override void CollectDataFromChildren ()
        {
            switch ((ChildNodes.First<ISqlNode>() as SqlKeyNode).Text.ToUpper())
            {
                case "+":
                    UnOp = UnOp.Plus;
                    break;
                case "-":
                    UnOp = UnOp.Minus;
                    break;
                case "~":
                    UnOp = UnOp.Tilde;
                    break;
                case "NOT":
                    UnOp = UnOp.Not;
                    break;
            }
        }
    }
}
