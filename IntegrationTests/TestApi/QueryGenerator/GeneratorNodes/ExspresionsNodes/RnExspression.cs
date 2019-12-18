using System;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes.ExspresionsNodes
{
    public partial class ExspressionNode
    {
        private class RnExspresion : ExspressionNode
        {
            private RnExspresion (bool isneedbracers) : base(isneedbracers)
            {
            }

            private RnExspresion (NameSpace ns, int maxdepth, ColumnType type = ColumnType.Bool, bool isusingid = false, string table = "") : base(ns, maxdepth, type, isusingid, table)
            {
            }
        }
    }
}
