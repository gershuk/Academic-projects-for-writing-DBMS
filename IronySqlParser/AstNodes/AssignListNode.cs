using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class AssignmentListNode : SqlNode
    {
        public List<AssignmentNode> Assignments { get; set; }

        public override void CollectDataFromChildren () => Assignments = FindAllChildNodesByType<AssignmentNode>();
    }
}
