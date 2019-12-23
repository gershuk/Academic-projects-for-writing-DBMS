namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes.ExspresionsNodes
{
    public partial class ExspressionNode
    {
        private class ArifmExspresion : ExspressionNode
        {
            private static readonly string[] _binoperators = { "+", "-", "*", "/", "%", "^" };
            private static readonly int[] _binoperatorsfr = { 3, 3, 3, 3, 2, 1 };
            private static readonly string[] _unoperators = { "+", "-", "" };
            private static readonly int[] _unoperatorsfr = { 1, 20, 100 };
            public ArifmExspresion (NameSpace ns, int maxdepth, bool isint, bool isneedbracers, bool isusingid = false, string table = "") : base(isneedbracers)
            {
                var chooser = new FrequencyRandomizer();
                if (isusingid)
                {
                    chooser.Insert(0, 7);//using id
                }
                chooser.Insert(1, 3);//as value
                isint = isint || _generator.NextDouble() < 0.3;// Double -> Int
                var value = isint ? _generator.Next(255555).ToString() : (_generator.Next(255555 / 10 * 7) * _generator.NextDouble()).ToString();
                var op = 0;
                if (_generator.NextDouble() * maxdepth <= valuechance * maxdepth)
                {
                    switch (chooser.GetRandom())
                    {
                        case 0:
                            _exspresion = isint ? ns.GetIntTableColumn(table)?._name : ns.GetDoubleTableColumn(table)?._name;
                            if (_exspresion == null)
                            {
                                _exspresion = value;
                            }
                            break;
                        case 1:
                            _exspresion = value;
                            break;
                    }
                }
                else
                {
                    var binopchooser = new FrequencyRandomizer();// "+", "-" , "*" , "/" , "%"
                    for (var i = 0; i < _binoperators.Length; i++)
                    {
                        binopchooser.Insert(i, _binoperatorsfr[i]);
                    }
                    if (!isint)
                    {
                        binopchooser.Remove(4);
                        binopchooser.Remove(5);
                    }
                    else
                    {
                        binopchooser.Remove(3);
                    }
                    op = binopchooser.GetRandom();
                    switch (chooser.GetRandom())
                    {
                        case 0:
                            {
                                _exspresion = $"{new ArifmExspresion(ns, maxdepth - 1, isint, op > 1, true, table)}"
                                + $"{_binoperators[op]}"
                                + $"{new ArifmExspresion(ns, maxdepth - 1, isint, op > 1, true, table)}";
                                break;
                            }
                        case 1:
                            _exspresion = $"{new ArifmExspresion(ns, maxdepth - 1, isint, op > 1, false, table)}"
                                + $"{_binoperators[op]}"
                                + $"{new ArifmExspresion(ns, maxdepth - 1, isint, op > 1, false, table)}";
                            break;
                    }
                }
                var unopchooser = new FrequencyRandomizer();// "+", "-" , nothing
                for (var i = 0; i < _unoperators.Length; i++)
                {
                    unopchooser.Insert(i, _unoperatorsfr[i]);
                }
                op = unopchooser.GetRandom();
                if (op != 2)
                {
                    _exspresion = $"({_unoperators[unopchooser.GetRandom()]}({_exspresion}))";
                }
                _exspresion = _exspresion.Replace(',', '.');
            }
        }
    }
}
