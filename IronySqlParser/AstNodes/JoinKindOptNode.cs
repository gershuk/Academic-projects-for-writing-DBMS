using System.Linq;

namespace IronySqlParser.AstNodes
{
    public enum JoinKind
    {
        Empty,
        Inner,
        Left,
        Right
    }

    class JoinKindOptNode : SqlNode
    {
        public JoinKind JoinKindOpt { set; get; }

        public override void CollectInfoFromChild() => JoinKindOpt = ParseEnum<JoinKind>((ChildNodes.First<ISqlNode>() as SqlKeyNode).Text);
    }
}
