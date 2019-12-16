namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes.ExspresionsNodes
{
    public partial class ExspressionNode
    {
        private class LogicExspresion : ExspressionNode
        {

            protected const double valuechance = 0.1;

            private static readonly string[] _binoperators = {"or", "and" };
            private static readonly int[] _binoperatorsfr = { 3,7};
            private static readonly string[] _unoperators = { "~", "not", "" };
            private static readonly int[] _unoperatorsfr = { 2, 2, 10 };

            public LogicExspresion (NameSpace ns, int maxdepth, bool isneedbracers, bool isusingid = false, string table = "") : base(isneedbracers)
            {
                var idvaluechooser = new FrequencyRandomizer();
                if (isusingid)
                {
                    idvaluechooser.Insert(0, 7);//using id
                }

                idvaluechooser.Insert(1, 3);//as value
                if (_generator.NextDouble() * maxdepth <= valuechance * maxdepth)
                {
                    switch (idvaluechooser.GetRandom())
                    {
                        case 0:
                            _exspresion = new CompExspresion(ns, maxdepth-1, true & isusingid, table).ToString();
                            break;
                        case 1:
                            _exspresion = new CompExspresion(ns, maxdepth-1, false, table).ToString();
                            break;
                    }
                }
                else
                {
                    var binopchooser = new FrequencyRandomizer();// "and", "or"
                    for (var i = 0; i < _binoperators.Length; i++)
                    {
                        binopchooser.Insert(i, _binoperatorsfr[i]);
                    }
                    var op = binopchooser.GetRandom();
                    switch (idvaluechooser.GetRandom())
                    {
                        case 0:
                            {
                                _exspresion = $"{new LogicExspresion(ns, maxdepth - 1, op > 0, true & isusingid, table)}"
                                + $"{_binoperators[op]}"
                                + $"{new LogicExspresion(ns, maxdepth - 1, op > 0, true & isusingid, table)}";
                                break;
                            }
                        case 1:
                            _exspresion = $"{new LogicExspresion(ns, maxdepth - 1, op > 0, false, table)}"
                                + $"{_binoperators[op]}"
                                + $"{new LogicExspresion(ns, maxdepth - 1, op > 0, false, table)}";
                            break;
                    }
                    var unopchooser = new FrequencyRandomizer();// "+", "-" , nothing
                    for (var i = 0; i < _unoperators.Length; i++)
                    {
                        unopchooser.Insert(i, _unoperatorsfr[i]);
                    }
                    op = unopchooser.GetRandom();
                    if (op != 2)
                    {
                        _exspresion = $"{_unoperators[unopchooser.GetRandom()]}({_exspresion})";
                    }

                }
            }
        }
    }
}
