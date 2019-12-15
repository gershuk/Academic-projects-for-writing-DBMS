using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes
{
    class InsertNode : IBaseNode
    {
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
        private List<string> _columns;
        private List<InsertDataNode> _values;
        InsertNode (NameSpace ns, int maxdepth)
        {

        }

        public override string ToString ()
        {
            return $"insert into {_id} {String.Join(", ", _columns)}" +
                $"values ({String.Join("), (", _values)})";
        }
    }
}
