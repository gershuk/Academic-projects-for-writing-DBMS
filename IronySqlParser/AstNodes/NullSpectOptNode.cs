namespace IronySqlParser.AstNodes
{
    public enum NullSpecOpt
    {
        Null,
        NotNull,
        Empty
    }

    public class NullSpectOptNode : SqlNode
    {
        public NullSpecOpt NullSpecOpt { get; set; }

        public override void CollectInfoFromChild()
        {
            var state = "";
            foreach (SqlKeyNode child in ChildNodes)
            {
                state += child.Text;
            }

            if (state == "")
            {
                state = "Empty";
            }

            NullSpecOpt = ParseEnum<NullSpecOpt>(state);
        }
    }
}
