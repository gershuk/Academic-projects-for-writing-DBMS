using System;
using System.Collections.Generic;
using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class UnExprNode : OperatorNode
    {
        private OperatorNode _childOperator;
        private UnOp _unOp;

        public override dynamic Calc (Dictionary<Id, dynamic> variables) =>
            _unOp switch
            {
                UnOp.Plus => +_childOperator?.Calc(variables),
                UnOp.Minus => -_childOperator?.Calc(variables),
                UnOp.Not => !_childOperator?.Calc(variables),
                UnOp.Tilde => ~_childOperator?.Calc(variables),
                _ => throw new NotImplementedException()
            };


        public override void CollectInfoFromChild ()
        {
            VariablesNames = new List<Id>();

            _unOp = FindFirstChildNodeByType<UnOpNode>().UnOp;

            var operatorNode = FindAllChildNodesByType<OperatorNode>();

            if (operatorNode.Count > 0)
            {
                _childOperator = operatorNode[0];
                GetAllValuesNamesFromNode(operatorNode[0]);
            }
        }
    }
}
