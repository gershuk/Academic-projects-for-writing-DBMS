using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes
{
    class InsertNode : IBaseNode
    {
        private const double _columnlistchance = 0.5;
        class InsertDataNode : IBaseNode
        {
            public InsertDataNode (NameSpace ns, int maxdepth, int minlength, int maxlength)
            {

            }

            public override string ToString ()
            {
                throw new NotImplementedException();
            }
        }

        private WhereNode _where;
        private string _id;
        private HashSet<Column> _columns;
        private List<InsertDataNode> _values;
        InsertNode (NameSpace ns, int maxdepth)
        {
            _id = ns.GetTableName();
            if (_generator.NextDouble() < _columnlistchance)
            {
                _columns = Enumerable.Range(0, _generator.Next(50) + 1).Select(_ => ns.GetTableColumn(_id)).ToHashSet();
            }

        }

        public override string ToString ()
        {
            return $"insert into {_id} " +
                $"{(_columns.Count>0?$"({String.Join(", ", _columns)})":"")}" +
                $"values ({String.Join("), (", _values)})";
        }
    }
}
