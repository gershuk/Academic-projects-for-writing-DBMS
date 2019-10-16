using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronySqlParser.AstNodes
{
    class UnExprNode : OperatorNode
    {
        private OperatorNode _childOperator;
        private UnOp _unOp;

        public override dynamic Calc() =>
            _unOp switch
            {
                UnOp.Plus => _childOperator == null ? +LeftValue.Data() : +_childOperator.Calc(),
                UnOp.Minus => _childOperator == null ? -LeftValue.Data() : -_childOperator.Calc(),
                UnOp.Not => _childOperator == null ? !LeftValue.Data() : !_childOperator.Calc(),
                UnOp.Tilde => _childOperator == null ? ~LeftValue.Data() : ~_childOperator.Calc()
            };


        public override void CollectInfoFromChild()
        {
            Variables = new Dictionary<string, Variable>();

            _unOp = FindChildNodesByType<UnOpNode>()[0].UnOp;
            var numberNode = FindChildNodesByType<NumberNode>();
            var stringLiteralNode = FindChildNodesByType<StringLiteralNode>();
            var idNod = FindChildNodesByType<IdNode>();
            var operatorNode = FindChildNodesByType<OperatorNode>();

            if (numberNode.Count > 0)
            {
                switch (numberNode[0].NumberType)
                {
                    case NumberType.Double:
                        LeftValue = new Variable { Data = numberNode[0].NumberDouble };
                        break;
                    case NumberType.Int:
                        LeftValue = new Variable { Data = numberNode[0].NumberInt };
                        break;
                };

            }


            if (stringLiteralNode.Count > 0)
            {
                LeftValue = new Variable { Data = stringLiteralNode[0].StringLiteral };
            }

            if (idNod.Count > 0)
            {
                var variable = new Variable { Name = idNod[0].Id };
                Variables.Add(idNod[0].Id, variable);
                LeftValue = variable;
            }

            if (operatorNode.Count > 0)
            {
                _childOperator = operatorNode[0];
                foreach (var variable in Variables)
                {
                    Variables.Add(variable.Key,variable.Value);
                }              
            }
        }
    }
}
