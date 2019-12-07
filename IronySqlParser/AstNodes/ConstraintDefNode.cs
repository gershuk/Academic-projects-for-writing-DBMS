namespace IronySqlParser.AstNodes
{
    public class ConstraintDefNode : SqlNode
    {
        public string ConstraintState { get; set; }

        public override void CollectInfoFromChild ()
        {
            ConstraintState = "";

            foreach (var child in ChildNodes)
            {
                switch (child)
                {
                    case StringLiteralNode literalNode:
                        ConstraintState += literalNode.StringLiteral + " ";
                        break;
                    case SqlKeyNode keyNode:
                        ConstraintState += keyNode.Text + " ";
                        break;
                }
            }

            ConstraintState = ConstraintState.TrimEnd(' ');
        }
    }
}
