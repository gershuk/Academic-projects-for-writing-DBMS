namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes.ExspresionsNodes
{
    public partial class ExspressionNode
    {
        private class ArifmExspresion : ExspressionNode
        {
            private readonly string[] _operators = { "+", "-", "*", "/", "%" };
            public ArifmExspresion (NameSpace ns, int maxdepth, bool isint, bool isneedbracers, bool isusingid = false, string table = ""):base(isneedbracers)
            {
                var chooser = new FrequencyRandomizer();
                if (isusingid)
                {
                    chooser.Insert(0, 7);//id
                }
                chooser.Insert(1, 3);//value
                var value = isint ? _generator.Next().ToString() : (_generator.Next() * _generator.NextDouble()).ToString();
                if (_generator.NextDouble() * maxdepth <= valuechance * maxdepth)
                {
                    switch (chooser.GetRandom())
                    {
                        case 0:
                            {
                                _exspresion = isint ? ns.GetIntTableColumn(table)?._name : ns.GetDoubleTableColumn(table)?._name;
                                if (_exspresion == null)
                                {
                                    _exspresion = value;
                                }
                                break;
                            }
                        case 1:
                            _exspresion = value;
                            break;
                    }
                }
                else
                {
                    var opchooser = new FrequencyRandomizer();// "+", "-" , "*" , "/" , "%"
                    opchooser.Insert(0, 3); // +
                    opchooser.Insert(1, 3); // -
                    opchooser.Insert(2, 5); // *
                    if (!isint)
                    {
                        opchooser.Insert(3, 5); // /
                    }
                    else
                    {
                        opchooser.Insert(4, 5); // %
                    }
                    var op = opchooser.GetRandom();
                    switch (chooser.GetRandom())
                    {
                        case 0:
                            {
                                _exspresion = $"{new ArifmExspresion(ns, maxdepth - 1, isint, op>1, true, table)}"
                                + $"{_operators[op]}"
                                + $"{new ArifmExspresion(ns, maxdepth - 1, isint, op > 1, true, table)}";
                                break;
                            }
                        case 1:
                            _exspresion = $"{new ArifmExspresion(ns, maxdepth - 1, isint, op > 1, false, table)}"
                                + $"{_operators[op]}"
                                + $"{new ArifmExspresion(ns, maxdepth - 1, isint, op > 1, false, table)}";
                            break;
                    }
                }
            }
        }
    }
}
