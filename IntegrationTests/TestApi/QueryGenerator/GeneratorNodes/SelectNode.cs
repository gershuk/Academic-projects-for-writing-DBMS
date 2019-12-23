using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes
{
    class SelectNode : IBaseNode
    {
        private NameSpace _nameSpace;
        private int _maxdepth;

        public SelectNode (NameSpace nameSpace, int maxdepth)
        {
            _nameSpace = nameSpace;
            _maxdepth = maxdepth;
        }

        public override string ToString ()
        {
            return "";
        }
    }
}
