using System.Collections.Generic;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public abstract class OperatorNode : SqlNode
    {
        public List<Id> VariablesNames { get; set; }

        public dynamic Value { get; set; }

        public abstract dynamic Calc (Dictionary<Id, dynamic> variables);

        public OperatorNode ()
        {
            VariablesNames = new List<Id>();
        }

        protected void GetAllValuesNamesFromNode (OperatorNode expNode)
        {
            foreach (var variable in expNode.VariablesNames)
            {
                var names = new Dictionary<Id, bool>();

                if (!names.TryGetValue(variable, out _))
                {
                    VariablesNames.Add(variable);
                    names.Add(variable, true);
                }
            }
        }
    }
}
