using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class IdLinkNode : SqlNode
    {
        public Id TableName { get; private set; }
        public override void CollectInfoFromChild ()
        {
            var sqlCommandNode = FindFirstChildNodeByType<SqlCommandNode>();
            TableName = sqlCommandNode?.ReturnedTableName;
            TableName ??= new Id(FindFirstChildNodeByType<IdNode>().Id);
        }
    }
}
