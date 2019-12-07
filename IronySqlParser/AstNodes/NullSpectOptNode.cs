using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class NullSpectOptNode : SqlNode
    {
        public NullSpecOpt NullSpecOpt { get; set; }

        public override void CollectInfoFromChild ()
        {
            var state = "";
            foreach (SqlKeyNode child in ChildNodes)
            {
                state += child.Text;
            }

            if (state.Length == 0)
            {
                state = "Empty";
            }

            NullSpecOpt = ParseEnum<NullSpecOpt>(state);
        }
    }
}
