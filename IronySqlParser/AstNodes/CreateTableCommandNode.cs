using System.Collections.Generic;
using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public class CreateTableCommandNode : SqlCommandNode
    {
        public List<string> TableName { get; set; }
        public List<FieldDefNode> FieldDefList { get; set; }

        public override void CollectInfoFromChild()
        {
            TableName = FindFirstChildNodeByType<IdNode>().Id;
            FieldDefList = FindFirstChildNodeByType<FieldDefListNode>().FieldDefList;
        }

        public override List<TableLock> GetCommandInfo() => new List<TableLock>() { };
    }
}
