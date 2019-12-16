using System.Linq;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class JoinKindOptNode : SqlNode
    {
        public JoinKind JoinKindOpt { set; get; } = JoinKind.Empty;

        public override void CollectDataFromChildren ()
        {
            if (ChildNodes.Count() > 0)
            {
                JoinKindOpt = ParseEnum<JoinKind>((ChildNodes.First<ISqlNode>() as SqlKeyNode).Text);
            }
        }
    }
}
