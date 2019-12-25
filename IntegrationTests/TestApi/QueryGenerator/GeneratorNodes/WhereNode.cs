using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes
{
    class WhereNode : IBaseNode
    {
        private string _exspression;
        public WhereNode (NameSpace ns,int maxdepth, string table, bool isusingid)
        {
            if (_generator.NextDouble()<0.5)
            {
                _exspression = new ExspresionsNodes.ExspressionNode(ns, maxdepth, ColumnType.Bool, isusingid, table);
            }
            else
            {
                _exspression = "(1=1)";
            }
        }

        public override string ToString ()
        {
            return $"where {_exspression}";
        }
    }
}
