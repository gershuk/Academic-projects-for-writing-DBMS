using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes
{
    class WhereNode : IBaseNode
    {
        private NameSpace _ns;

        public WhereNode (NameSpace ns)
        {
            _ns = ns;
        }

        public override string ToString ()
        {
            throw new NotImplementedException();
        }
    }
}
