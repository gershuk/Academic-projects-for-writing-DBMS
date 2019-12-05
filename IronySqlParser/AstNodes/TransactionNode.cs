namespace IronySqlParser.AstNodes
{
    public class TransactionNode : SqlNode
    {
        public TransactionBeginOptNode TransactionBeginOptNode { get; set; }
        public TransactionEndOptNode TransactionEndOptNode { get; set; }
        public StmtListNode StmtListNode { get; set; }
        public SqlCommandNode SqlCommandNode { get; set; }

        public override void CollectInfoFromChild ()
        {
            TransactionBeginOptNode = FindFirstChildNodeByType<TransactionBeginOptNode>();
            TransactionEndOptNode = FindFirstChildNodeByType<TransactionEndOptNode>()
                ?? new TransactionEndOptNode() { TransactionEndType = DataBaseType.TransactionEndType.Commit };
            StmtListNode = FindFirstChildNodeByType<StmtListNode>();
            SqlCommandNode = FindFirstChildNodeByType<SqlCommandNode>();
        }
    }
}
