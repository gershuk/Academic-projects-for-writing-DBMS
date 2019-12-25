using System;
using System.Collections.Generic;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class ExpressionNode : OperatorNode
    {
        private string _variableName;
        private OperatorNode _childOperator;

        public override dynamic Calc (Dictionary<string, dynamic> variables)
        {
            if (!_wasСalculated || !ConstOnly)
            {
                if (_variableName != null)
                {
                    if (!variables.TryGetValue(_variableName, out var value))
                    {
                        throw new NullReferenceException();
                    }

                    Value = value;
                }

                _cachedValue = _childOperator == null ? Value : _childOperator.Calc(variables);
            }

            _wasСalculated = true;
            return _cachedValue;
        }

        public override void CollectDataFromChildren ()
        {
            VariablesNames = new List<string>();
            ConstOnly = true;

            var numberNode = FindFirstChildNodeByType<NumberNode>();
            var stringLiteralNode = FindFirstChildNodeByType<StringLiteralNode>();
            var idNod = FindFirstChildNodeByType<IdNode>();
            _childOperator = FindFirstChildNodeByType<OperatorNode>();

            if (numberNode != null)
            {
                switch (numberNode.NumberType)
                {
                    case NumberType.Double:
                        Value = numberNode.NumberDouble;
                        break;
                    case NumberType.Int:
                        Value = numberNode.NumberInt;
                        break;
                };
            }

            if (stringLiteralNode != null)
            {
                Value = stringLiteralNode.StringLiteral;
            }

            if (idNod != null)
            {
                _variableName = idNod.Id.ToString();
                VariablesNames.Add(_variableName.ToString());
                ConstOnly = false;

                IsCompressed = true;
                AddVariableBorder(_variableName, new VariableBorder());
            }

            if (_childOperator != null)
            {
                IsCompressed = _childOperator.IsCompressed;

                if (IsCompressed)
                {
                    foreach (var variableBorder in _childOperator.VariablesBorder)
                    {
                        AddVariableBorder(variableBorder.Key, (VariableBorder)variableBorder.Value.Clone());
                    }
                }

                ConstOnly = _childOperator.ConstOnly;
                GetAllValuesNamesFromNode(_childOperator);
            }
        }
    }
}
