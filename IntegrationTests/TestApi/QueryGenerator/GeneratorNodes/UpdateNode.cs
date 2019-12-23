using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes
{
    public class UpdateNode : IBaseNode
    {
        private string _id;
        private Column _column;
        private ExspresionsNodes.ExspressionNode _assigment;
        private WhereNode _where;
        public UpdateNode (NameSpace ns, int maxdepth)
        {
            _id = ns.GetTableName();
            _column = ns.GetTableColumn(_id);
            _assigment = new ExspresionsNodes.ExspressionNode(ns, maxdepth, _column._type, true, _id);
            _where = new WhereNode(ns, maxdepth, _id, true);
        }

        public override string ToString ()
        {
            return $"update {_id} set {_column._name}={_assigment} {_where}";
        }
    }
}
