using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes
{
    class CreateTableNode : IBaseNode
    {
        private string _tablename;

        CreateTableNode(NameSpace ns, int maxdepth)
        {
            _tablename = ns.GetTableName();
        }

        public override string ToString ()
        {
            return $"create table {_tablename};";
        }
    }
}
