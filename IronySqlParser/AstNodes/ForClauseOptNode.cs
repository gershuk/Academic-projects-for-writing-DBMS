namespace IronySqlParser.AstNodes
{
    public class ForClauseOptNode:SqlNode
    {
        public TimeSelectorNode TimeSelectorNode { get; set; }

        public override void CollectDataFromChildren () => TimeSelectorNode = FindFirstChildNodeByType<SystemTimeOptNode>()?.TimeSelectorNode;
    }
}
