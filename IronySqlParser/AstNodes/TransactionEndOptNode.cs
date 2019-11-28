using System.Linq;

namespace IronySqlParser.AstNodes
{
    public enum TransactionEndType
    {
        Commit,
        Rollback
    }

    public class TransactionEndOptNode : SqlNode
    {
        public TransactionEndType TransactionEndType { get; set; }

        public override void CollectInfoFromChild() => TransactionEndType = ParseEnum<TransactionEndType>(ChildNodes.First().Tokens.First().Text);
    }
}
