using System.Collections.Generic;
using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public abstract class OperatorNode : SqlNode
    {
        public Dictionary<List<string>, Variable> Variables { get; set; }
        public Variable Value { get; set; }

        public abstract dynamic Calc();
    }
}
