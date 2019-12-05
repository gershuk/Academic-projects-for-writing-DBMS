using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class IdLinkNode : SqlNode
    {
        public List<string> TableName { get; private set; }
        public override void CollectInfoFromChild ()
        {
            var sqlCommandNode = FindFirstChildNodeByType<SqlCommandNode>();
            TableName = sqlCommandNode?.ReturnedTableName;
            TableName ??= FindFirstChildNodeByType<IdNode>().Id;
        }
    }
}
