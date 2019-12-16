using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntegrationTests.TestApi.QueryGenerator.GeneratorNodes;
using IntegrationTests.TestApi.QueryGenerator.GeneratorNodes.ExspresionsNodes;

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

        public QueryGenerator(NameSpace ns )
        {
            _nameSpace = ns;
        }
        public QueryGenerator ()
        {
            _nameSpace = new NameSpace();
        }

        public string GenerateQuery()
        {
            return new CreateTableNode(_nameSpace,10).ToString(); 
        }
        public string Expression ()
        {
            return new ExspressionNode(_nameSpace, 1, ColumnType.Bool,false, _nameSpace.GetTableName()).ToString()+"\n\n\n"+new ExspressionNode(_nameSpace, 5, ColumnType.Double, true, _nameSpace.GetTableName()).ToString();
        }

    }
}
