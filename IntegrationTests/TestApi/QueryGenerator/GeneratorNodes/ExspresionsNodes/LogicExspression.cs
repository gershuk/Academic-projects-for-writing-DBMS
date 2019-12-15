namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes.ExspresionsNodes
{
    public partial class ExspressionNode
    {
        private class LogicExspresion : ExspressionNode
        {
            private LogicExspresion (bool isneedbracers) : base(isneedbracers)
            {
            }

            private LogicExspresion (NameSpace ns, int maxdepth, ColumnType type = ColumnType.Bool, bool isusingid = false, string table = "") : base(ns, maxdepth, type, isusingid, table)
            {
            }
        }
    }
}
