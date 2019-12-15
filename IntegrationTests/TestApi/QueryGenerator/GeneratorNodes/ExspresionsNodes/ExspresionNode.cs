using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes.ExspresionsNodes
{
    public partial class ExspresionNode : IBaseNode
    {
        protected string _exspresion;
        protected static Random _generator = default;
        protected const double valuechance = 0.3;
        private readonly double _bracerschance = 0.3;
        protected readonly bool _inbracers;
        ExspresionNode (bool isneedbracers)
        {
            _inbracers = isneedbracers ? true : _generator.NextDouble() < _bracerschance;  
        }
        ExspresionNode (NameSpace ns, int maxdepth, ColumnType type = ColumnType.Bool, bool isusingid = false, string table = "")
        {
            switch (type)
            {
                case ColumnType.Double:
                    _exspresion = new ArifmExspresion(ns, maxdepth - 1, false, false, isusingid, table).ToString();
                    break;
                case ColumnType.Int:
                    _exspresion = new ArifmExspresion(ns, maxdepth - 1, true, false, isusingid, table).ToString();
                    break;
                case ColumnType.Char:

                    break;
                case ColumnType.Any:

                    break;
                case ColumnType.Bool:

                    break;
            }
        }

        public override string ToString ()
        {
            return Bracers(_exspresion);
        }


        protected string Bracers (string val) => _inbracers ? $"({val})" : val;
    }
}
