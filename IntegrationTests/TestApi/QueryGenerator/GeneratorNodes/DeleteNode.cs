using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes
{
    class DeleteNode : IBaseNode
    {
        private WhereNode _where;
        private string _id;
        public DeleteNode (NameSpace ns, int maxdepth)
        {
            _id = ns.GetTableName();
            _where = new WhereNode(ns);
        }

        public override string ToString ()
        {
            return $"delete from {_id} {_where};";
        }
    }
}
