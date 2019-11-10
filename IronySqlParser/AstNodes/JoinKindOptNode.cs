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

    internal class JoinKindOptNode : SqlNode
    {
        public JoinKind JoinKindOpt { set; get; } = JoinKind.Empty;

        public override void CollectInfoFromChild()
        {
            if (ChildNodes.Count() > 0)
            {
                JoinKindOpt = ParseEnum<JoinKind>((ChildNodes.First<ISqlNode>() as SqlKeyNode).Text);
            }
        }
    }
}
