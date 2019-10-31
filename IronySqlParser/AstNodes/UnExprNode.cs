using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class UnExprNode : OperatorNode
    {
        private OperatorNode _childOperator;
        private UnOp _unOp;

        public override dynamic Calc() =>
            _unOp switch
            {
                UnOp.Plus => _childOperator == null ? +Value.Data() : +_childOperator.Calc(),
                UnOp.Minus => _childOperator == null ? -Value.Data() : -_childOperator.Calc(),
                UnOp.Not => _childOperator == null ? !Value.Data() : !_childOperator.Calc(),
                UnOp.Tilde => _childOperator == null ? ~Value.Data() : ~_childOperator.Calc()
            };


        public override void CollectInfoFromChild()
        {
            Variables = new Dictionary<List<string>, Variable>();

            _unOp = FindFirstChildNodeByType<UnOpNode>().UnOp;

            var operatorNode = FindAllChildNodesByType<OperatorNode>();

            if (operatorNode.Count > 0)
            {
                _childOperator = operatorNode[0];
                foreach (var variable in Variables)
                {
                    Variables.Add(variable.Key, variable.Value);
                }
            }
        }
    }
}
