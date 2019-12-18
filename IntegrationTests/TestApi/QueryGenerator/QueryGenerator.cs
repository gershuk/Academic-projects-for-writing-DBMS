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
        private const int _maxdepth = 2;
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

       

        public QueryGenerator ():this(new NameSpace())
        {
        }

        public string GenerateQuery()
        {
            var _querychooser = new FrequencyRandomizer();
            _querychooser.Insert(1, 3);
            if (_nameSpace.IsTablesExists)
            {
                _querychooser.Insert(2, 3);///Drop
                _querychooser.Insert(3, 10);///Insert
                _querychooser.Insert(4, 8);///Delete
                _querychooser.Insert(5, 10);///Update
                _querychooser.Insert(6, 0);///Select=
            }



            switch (_querychooser.GetRandom())
            {
                case 1:
                    return Create();
                case 2:
                    return Drop();
                case 3:
                    return Insert();
                case 4:
                    return Delete();
                case 5:
                    return Update();
                case 6:
                    return Select();
            }   
            return Create();
        }

        private string Create ()
        {
            return new CreateTableNode(_nameSpace, _maxdepth).ToString();
        }

        private string Insert ()
        {
            return new InsertNode(_nameSpace, _maxdepth).ToString();
        }

        private string Drop ()
        {
            return new DropTableNode(_nameSpace, _maxdepth).ToString();
        }

        private string Delete ()
        {
            return new DeleteNode(_nameSpace, _maxdepth).ToString();
        }

        private string Update ()
        {
            return new UpdateNode(_nameSpace, _maxdepth).ToString();
        }

        private string Select ()
        {
            return new SelectNode(_nameSpace, _maxdepth).ToString();
        }

    }
}
