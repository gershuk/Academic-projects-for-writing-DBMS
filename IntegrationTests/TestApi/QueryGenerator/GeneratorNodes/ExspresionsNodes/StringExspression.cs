namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes.ExspresionsNodes
{
    public partial class ExspressionNode
    {
        class StringExspresion : ExspressionNode
        {
            private StringExspresion (bool isneedbracers) : base(isneedbracers)
            {
            }

            private StringExspresion (NameSpace ns, int maxdepth, ColumnType type = ColumnType.Bool, bool isusingid = false, string table = "") : base(ns, maxdepth, type, isusingid, table)
            {
            }
        }
    }
}
