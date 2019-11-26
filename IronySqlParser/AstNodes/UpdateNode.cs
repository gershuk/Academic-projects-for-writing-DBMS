using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    internal class UpdateNode : SqlNode
    {
        public List<string> TableName { get; set; }
        public List<AssignmentNode> Assignments { get; set; }
        public ExpressionNode WhereExpression { get; set; }

        public override void CollectInfoFromChild()
        {
            TableName = FindFirstChildNodeByType<IdNode>()?.Id;
            Assignments = FindFirstChildNodeByType<AssignmentListNode>()?.Assignments;
            WhereExpression = FindFirstChildNodeByType<WhereClauseNode>()?.Expression;
        }
    }
}
