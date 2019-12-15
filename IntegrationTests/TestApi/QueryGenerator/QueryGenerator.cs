using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator
{
    public class QueryGenerator
    {
        private NameSpace _nameSpace;
        
        public bool IsRandom
        {
            get => _nameSpace.IsRandom;
            set => _nameSpace.NotExistedParam = value ? 0.4 : 0;
        }

        public double NotExistedParam
        {
            get => _nameSpace.NotExistedParam;
            set => _nameSpace.NotExistedParam = value;
        }

        public QueryGenerator(NameSpace ns = default)
        {
            _nameSpace = ns;
        }

        public string GenerateQuery()
        {

        }

    }
}
