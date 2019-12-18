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
            private int _length;
            private List<string> _values = new List<string>(); 
            public InsertDataNode (NameSpace ns, int maxdepth, int minlength, int maxlength, List<Column> columns)
            {
                _length = _generator.Next(minlength, maxlength);
                for (var i = 0; i< _length;i++)
                {
                    _values.Add(new ExspresionsNodes.ExspressionNode(ns, maxdepth, columns[i]._type).ToString());
                }
            }

            public override string ToString ()
            {
                return String.Join(", ", _values);
        }
        }
        private bool _usecolumns;
        private string _id;
        private List<Column> _columns;
        private List<InsertDataNode> _values;
        public InsertNode (NameSpace ns, int maxdepth)
        {
            _id = ns.GetTableName();
            //_usecolumns = _generator.NextDouble() < 0.2;
            _usecolumns = true;
             _columns = Enumerable.Range(0, _generator.Next(10) + 2).Select(_ => ns.GetTableColumn(_id)).ToHashSet().ToList();
            _values = Enumerable.Range(0, _generator.Next(5) + 1).Select(_ => new InsertDataNode(ns, maxdepth, _usecolumns? _columns.Count: 2, _columns.Count, _columns)).ToList();
        }

        public override string ToString ()
        {
            return $"insert into {_id} " +
                $"{(_usecolumns?$"({String.Join(", ", _columns.Select(i=>i._name))})":"")} " +
                $"values ({String.Join("), (", _values)})";
        }
    }
}
