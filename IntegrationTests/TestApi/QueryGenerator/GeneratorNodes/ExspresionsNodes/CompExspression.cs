namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes.ExspresionsNodes
{
    public partial class ExspressionNode
    {
        private class CompExspresion : ExspressionNode
        {
            private CompExspresion (bool isneedbracers) : base(isneedbracers)
            {
            }

            private CompExspresion (NameSpace ns, int maxdepth, ColumnType type = ColumnType.Bool, bool isusingid = false, string table = "") : base(ns, maxdepth, type, isusingid, table)
            {
            }
        }
    }
}
