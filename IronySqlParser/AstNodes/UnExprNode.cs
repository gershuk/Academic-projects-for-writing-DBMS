using System;
using System.Collections.Generic;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class UnExprNode : OperatorNode
    {
        private OperatorNode _childOperator;
        private UnOp _unOp;

        public override dynamic Calc (Dictionary<string, dynamic> variables) 
        {
            if (!_wasСalculated || !ConstOnly)
            {
                _cachedValue = _unOp switch
                {
                    UnOp.Plus => +_childOperator?.Calc(variables),
                    UnOp.Minus => -_childOperator?.Calc(variables),
                    UnOp.Not => !_childOperator?.Calc(variables),
                    UnOp.Tilde => ~_childOperator?.Calc(variables),
                    _ => throw new NotImplementedException()
                };
            }         

            _wasСalculated = true;
            return _cachedValue;
        }
            
        public override void CollectDataFromChildren ()
        {
            VariablesNames = new List<string>();

            _unOp = FindFirstChildNodeByType<UnOpNode>().UnOp;

            _childOperator = FindFirstChildNodeByType<OperatorNode>();

            if (_childOperator != null)
            {
                ConstOnly = _childOperator.ConstOnly;
                IsCompressed = ConstOnly;
                GetAllValuesNamesFromNode(_childOperator);
            }
        }
    }
}
