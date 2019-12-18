using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes.ExspresionsNodes
{
    
    public partial class ExspressionNode : IBaseNode
    {
        protected string _exspresion;
        protected const double valuechance = 0.3;
        private readonly double _bracerschance = 0.1;
        protected readonly bool _inbracers = false;

        ExspressionNode (bool isneedbracers)
        {
            _inbracers = isneedbracers ? true : _generator.NextDouble() < _bracerschance;  
        }
        public ExspressionNode (NameSpace ns, int maxdepth, ColumnType type = ColumnType.Bool, bool isusingid = false, string table = "")
        {
            switch (type)
            {
                case ColumnType.Double:
                    _exspresion = new ArifmExspresion(ns, maxdepth, false, false, isusingid, table).ToString();
                    break;
                case ColumnType.Int:
                    _exspresion = new ArifmExspresion(ns, maxdepth, true, false, isusingid, table).ToString();
                    break;
                case ColumnType.Char:
                    _exspresion = new StringExspresion(ns, maxdepth, isusingid, table).ToString();
                    break;
                case ColumnType.Bool:
                    _exspresion = new LogicExspresion(ns, maxdepth, false, isusingid, table).ToString();
                    break;
                case ColumnType.Any:

                    break;
            }
        }

        public override string ToString ()
        {
            return _inbracers ? $"({_exspresion})" : _exspresion;
        }


    }
}
