namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes.ExspresionsNodes
{
    public partial class ExspressionNode
    {
        private class CompExspresion : ExspressionNode
        {
            private static readonly string[] _operators = { "=", ">", "<", ">=", "<=", "<>", "!=", "!<", "!>" };
            private static readonly int[] _operatorsfr = { 7, 3, 3, 3, 3, 3, 3, 1, 1 };

            public CompExspresion (NameSpace ns, int maxdepth, bool isusingid = false, string table = "") : base(true)
            {
                var compchooser = new FrequencyRandomizer();

                compchooser.Insert(0, 7*2);// int int
                compchooser.Insert(1, 7*2);// int double
                compchooser.Insert(2, 1);// int string
                compchooser.Insert(3, 7*2);// double double
                compchooser.Insert(4, 7*2);// double int
                compchooser.Insert(5, 1);// double string
                compchooser.Insert(6, 1);// string string
                compchooser.Insert(7, 1);// string int 
                compchooser.Insert(8, 1);// string double
                var opchooser = new FrequencyRandomizer();

                for (var i = 0; i < _operators.Length; i++)
                {
                    opchooser.Insert(i, _operatorsfr[i]);
                }
                var _left = ColumnType.Any;
                var _right = ColumnType.Any;
                switch (compchooser.GetRandom())
                {
                    case 0:
                        _left = ColumnType.Int;
                        _right = ColumnType.Int;
                        break;
                    case 1:
                        _left = ColumnType.Int;
                        _right = ColumnType.Double;
                        break;
                    case 2:
                        _left = ColumnType.Int;
                        _right = ColumnType.Char;
                        break;
                    case 3:
                        _left = ColumnType.Double;
                        _right = ColumnType.Double;
                        break;
                    case 4:
                        _left = ColumnType.Double;
                        _right = ColumnType.Int;
                        break;
                    case 5:
                        _left = ColumnType.Double;
                        _right = ColumnType.Char;
                        break;
                    case 6:
                        _left = ColumnType.Char;
                        _right = ColumnType.Char;
                        break;
                    case 7:
                        _left = ColumnType.Char;
                        _right = ColumnType.Int;
                        break;
                    case 8:
                        _left = ColumnType.Char;
                        _right = ColumnType.Double;
                        break;
                }
                _exspresion = $"({new ExspressionNode(ns, maxdepth-1, _left, isusingid, table)})" +
                    $"{_operators[opchooser.GetRandom()]}" +
                    $"({new ExspressionNode(ns, maxdepth-1, _right, isusingid, table)})";
            }
        }
    }
}
