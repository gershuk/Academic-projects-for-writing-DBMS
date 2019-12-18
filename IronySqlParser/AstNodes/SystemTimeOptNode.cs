namespace IronySqlParser.AstNodes
{
    class SystemTimeOptNode : SqlNode
    {
        public TimeSelectorNode TimeSelectorNode { get; set; }

        public override void CollectDataFromChildren () => TimeSelectorNode = FindFirstChildNodeByType<TimeSelectorNode>();
    }
}
