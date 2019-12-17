using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class FromClauseNode : SqlNode
    {
        public Id IdList { get; set; }

        public override void CollectDataFromChildren ()
        {
            var idLinkNode = FindFirstChildNodeByType<IdLinkNode>();
            IdList = idLinkNode?.TableName;
        }
    }
}
