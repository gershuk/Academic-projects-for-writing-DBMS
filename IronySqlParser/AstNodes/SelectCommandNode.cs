using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class SelectCommandNode : SqlCommandNode
    {

        public List<List<string>> ColumnIdList { get; set; }
        public List<List<string>> TableIdList { get; set; }
        public ExpressionNode WhereExpression { get; set; }

        public override void CollectInfoFromChild()
        {
            ColumnIdList = FindFirstChildNodeByType<SelListNode>()?.IdList;
            TableIdList = FindFirstChildNodeByType<FromClauseNode>()?.IdList;
            WhereExpression = FindFirstChildNodeByType<WhereClauseNode>()?.Expression;
        }
    }
}

