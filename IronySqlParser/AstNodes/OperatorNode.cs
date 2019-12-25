using System;
using System.Collections.Generic;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public abstract class OperatorNode : SqlNode
    {
        public List<string> VariablesNames { get; set; }
        public dynamic Value { get; set; }
        public bool ConstOnly { get; set; }
        public bool IsCompressed { get; protected set; }
        public Dictionary<string, VariableBorder> VariablesBorder { get; protected set; }

        protected dynamic _cachedValue;
        protected bool _wasСalculated;

        public abstract dynamic Calc (Dictionary<string, dynamic> variables);

        public OperatorNode ()
        {
            IsCompressed = false;
            VariablesBorder = new Dictionary<string, VariableBorder>();
            VariablesNames = new List<string>();
            _wasСalculated = false;
        }

        protected void GetAllValuesNamesFromNode (OperatorNode expNode)
        {
            foreach (var variable in expNode.VariablesNames)
            {
                var names = new HashSet<string>();

                if (!names.TryGetValue(variable, out _))
                {
                    VariablesNames.Add(variable);
                    names.Add(variable);
                }
            }
        }

        protected void AddVariableBorder (string variableName, VariableBorder variableBorder)
        {
            if (VariablesBorder.TryGetValue(variableName, out var variable))
            {
                variable.LeftBorder = variableBorder.LeftBorder;
                variable.RightBorder = variableBorder.RightBorder;

                if (variable.LeftBorder == variableBorder.LeftBorder)
                {
                    variable.StrictLeft = variableBorder.StrictLeft;
                }

                if (variable.LeftBorder == variableBorder.LeftBorder)
                {
                    variable.StrictRight = variableBorder.StrictRight;
                }
            }
            else
            {
                VariablesBorder.Add(variableName, variableBorder);
            }
        }

        protected void EditVariableBorder (string variableName, VariableBorder variableBorder)
        {
            if (VariablesBorder.TryGetValue(variableName, out var variable))
            {
                variable.LeftBorder = variableBorder.LeftBorder;
                variable.RightBorder = variableBorder.RightBorder;

                if (variable.LeftBorder == variableBorder.LeftBorder)
                {
                    variable.StrictLeft = variableBorder.StrictLeft;
                }

                if (variable.LeftBorder == variableBorder.LeftBorder)
                {
                    variable.StrictRight = variableBorder.StrictRight;
                }
            }
            else
            {
                throw new Exception($"Variable {variableName} doesn't exist");
            }
        }
    }
}
