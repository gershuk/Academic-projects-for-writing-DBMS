using System.Collections.Generic;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public abstract class OperatorNode : SqlNode
    {
        public List<string> VariablesNames { get; set; }
        public dynamic Value { get; set; }
        public bool ConstOnly { get; set; }

        protected dynamic _cachedValue;
        protected bool _wasСalculated;

        public abstract dynamic Calc (Dictionary<string, dynamic> variables);

        public OperatorNode ()
        {
            VariablesNames = new List<string>();
            _wasСalculated = false;
        }

        protected void GetAllValuesNamesFromNode (OperatorNode expNode)
        {
            foreach (var variable in expNode.VariablesNames)
            {
                var names = new Dictionary<string, bool>();

                if (!names.TryGetValue(variable, out _))
                {
                    VariablesNames.Add(variable);
                    names.Add(variable, true);
                }
            }
        }
    }
}
