namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes.ExspresionsNodes
{
    public partial class ExspressionNode
    {
        class StringExspresion : ExspressionNode
        {
            public StringExspresion (NameSpace ns, int maxdepth, bool isusingid = false, string table = "") : base(false)
            {
                var idvaluechooser = new FrequencyRandomizer();
                if (isusingid)
                {
                    idvaluechooser.Insert(0, 7);//using id
                }
                idvaluechooser.Insert(1, 3);//as value
                if (_generator.NextDouble() * maxdepth <= valuechance * maxdepth)
                {
                    var value = $"'{NameSpace.RandomString()}'";
                    switch (idvaluechooser.GetRandom())
                    {
                        case 0:
                            _exspresion =  ns.GetCharTableColumn(table)?._name;
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
                    switch (idvaluechooser.GetRandom())
                    {
                        case 0:
                            {
                                _exspresion = $"{new StringExspresion(ns, maxdepth - 1, true, table)}"
                                + $"+"
                                + $"{new StringExspresion(ns, maxdepth - 1, true, table)}";
                                break;
                            }
                        case 1:
                            _exspresion = $"{new StringExspresion(ns, maxdepth - 1, false, table)}"
                                + $"+"
                                + $"{new StringExspresion(ns, maxdepth - 1, false, table)}";
                            break;
                    }
                }
            }
        }
    }
}
