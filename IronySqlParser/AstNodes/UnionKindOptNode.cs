using System.Linq;

namespace IronySqlParser.AstNodes
{
    public enum UnionKind
    {
        Empty,
        All
    }

    public class UnionKindOptNode : SqlNode
    {
        public UnionKind UnionKindOpt { set; get; } = UnionKind.Empty;

        public override void CollectInfoFromChild()
        {
            if (ChildNodes.Count() > 0)
            {
                UnionKindOpt = ParseEnum<UnionKind>((ChildNodes.First<ISqlNode>() as SqlKeyNode).Text);
            }
        }
    }
}
