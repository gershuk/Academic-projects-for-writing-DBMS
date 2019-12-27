using System;
using System.Collections.Generic;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class UnExprNode : OperatorNode
    {
        private OperatorNode _childOperator;
        private UnOp _unOp;
        private string _variableName;

        public override dynamic Calc (Dictionary<string, dynamic> variables)
        {
            if (!_wasСalculated || !ConstOnly)
            {
                _cachedValue = _unOp switch
                {
                    UnOp.Plus => _childOperator == null ? +Value : +_childOperator.Calc(variables),
                    UnOp.Minus => _childOperator == null ? -Value : -_childOperator.Calc(variables),
                    UnOp.Not => _childOperator == null ? !Value : !_childOperator.Calc(variables),
                    UnOp.Tilde => _childOperator == null ? ~Value : ~_childOperator.Calc(variables),
                    _ => throw new NotImplementedException()
                };
            }

            _wasСalculated = true;
            return _cachedValue;
        }

        public override void CollectDataFromChildren ()
        {
            var numberNode = FindFirstChildNodeByType<NumberNode>();
            var stringLiteralNode = FindFirstChildNodeByType<StringLiteralNode>();
            var idNod = FindFirstChildNodeByType<IdNode>();

            VariablesNames = new List<string>();

            _unOp = FindFirstChildNodeByType<UnOpNode>().UnOp;

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
                IsCompressed = true;
            }

            if (stringLiteralNode != null)
            {
                Value = stringLiteralNode.StringLiteral;
                IsCompressed = true;
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
                ConstOnly = _childOperator.ConstOnly;
                IsCompressed = ConstOnly;
                GetAllValuesNamesFromNode(_childOperator);
            }
        }
    }
}
