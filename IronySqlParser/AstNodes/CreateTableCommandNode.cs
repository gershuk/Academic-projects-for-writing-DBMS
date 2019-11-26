using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    internal class CreateTableCommandNode : SqlCommandNode
    {
        public List<string> TableName { get; set; }
        public List<FieldDefNode> FieldDefList { get; set; }

        public override void CollectInfoFromChild()
        {
            TableName = FindFirstChildNodeByType<IdNode>().Id;
            FieldDefList = FindFirstChildNodeByType<FieldDefListNode>().FieldDefList;
        }
    }
}
