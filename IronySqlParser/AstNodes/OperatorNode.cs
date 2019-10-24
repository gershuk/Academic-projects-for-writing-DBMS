using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class Variable
    {
        public dynamic Data { get; set; }
        public string Name { get; set; }
    }

    abstract public class OperatorNode : SqlNode
    {
        public Dictionary<string, Variable> Variables { get; set; }
        public Variable Value { get; set; }
       
        public abstract dynamic Calc();
    }


}
