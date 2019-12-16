using System;
using System.Collections.Generic;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class ExpressionNode : OperatorNode
    {
        private Id _variableName;
        private OperatorNode _childOperator;

        public override dynamic Calc (Dictionary<Id, dynamic> variables)
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

        public override void CollectInfoFromChild ()
        {
            VariablesNames = new List<Id>();
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
                _variableName = new Id(idNod.Id);
                VariablesNames.Add(_variableName);
                ConstOnly = false;
            }

            if (_childOperator != null)
            {
                ConstOnly = _childOperator.ConstOnly;
                GetAllValuesNamesFromNode(_childOperator);
            }
        }
    }
}
