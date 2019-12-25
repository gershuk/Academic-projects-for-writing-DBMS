using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class IdLinkNode : SqlNode
    {
        public Id TableName { get; private set; }
        public override void CollectDataFromChildren ()
        {
            var sqlCommandNode = FindFirstChildNodeByType<SqlCommandNode>();
            TableName = sqlCommandNode?.ReturnedTableName;
            TableName ??= FindFirstChildNodeByType<IdNode>().Id;
        }
    }
}
