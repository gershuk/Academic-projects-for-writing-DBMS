using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes
{
    class DropTableNode : IBaseNode
    {
        private string _tablename;

        public DropTableNode (NameSpace ns, int maxdepth)
        {
            _tablename = ns.GetTableName();
            ns.RemoveTable(_tablename);
        }

        public override string ToString ()
        {
            return $"drop table {_tablename};";
        }
    }
}
