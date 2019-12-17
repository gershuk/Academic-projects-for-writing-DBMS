using System.Linq;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class UnionKindOptNode : SqlNode
    {
        public UnionKind UnionKindOpt { set; get; } = UnionKind.Empty;

        public override void CollectDataFromChildren ()
        {
            if (ChildNodes.Count() > 0)
            {
                UnionKindOpt = ParseEnum<UnionKind>((ChildNodes.First<ISqlNode>() as SqlKeyNode).Text);
            }
        }
    }
}
