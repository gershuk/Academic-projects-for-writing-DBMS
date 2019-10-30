using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class UpdateNode : SqlNode
    {
        public List<string> TableName { get; set; }
        public List<AssignmentNode> Assignments { get; set; }
        public ExpressionNode WhereExpression { get; set; }

        public override void CollectInfoFromChild()
        {
            TableName = FindChildNodesByType<IdNode>()[0].Id;
            Assignments = FindChildNodesByType<AssignmentListNode>()[0].Assignments;
            WhereExpression = FindChildNodesByType<WhereClauseNode>()[0].Expression;
        }
    }
}
