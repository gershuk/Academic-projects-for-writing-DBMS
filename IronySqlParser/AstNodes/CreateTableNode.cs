using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class CreateTableNode : SqlNode
    {
        public string TableName { get; set; }
        public List<FieldDefNode> FieldDefList { get; set; }

        public override void CollectInfoFromChild()
        {
            TableName = FindChildNodesByType<IdNode>()[0].Id;
            FieldDefList = FindChildNodesByType<FieldDefListNode>()[0].FieldDefList;
        }
    }
}
