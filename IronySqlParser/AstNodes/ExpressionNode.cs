using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronySqlParser.AstNodes
{
    class ExpressionNode : OperatorNode
    {
        private OperatorNode _childOperator;
        public override dynamic Calc() => _childOperator == null ? Value.Data : _childOperator.Calc();

        public override void CollectInfoFromChild()
        {
            Variables = new Dictionary<string, Variable>();

            var numberNode = FindChildNodesByType<NumberNode>();
            var stringLiteralNode = FindChildNodesByType<StringLiteralNode>();
            var idNod = FindChildNodesByType<IdNode>();
            var operatorNode = FindChildNodesByType<OperatorNode>();

            if (numberNode.Count > 0)
            {
                switch (numberNode[0].NumberType)
                {
                    case NumberType.Double:
                        Value = new Variable { Data = numberNode[0].NumberDouble };
                        break;
                    case NumberType.Int:
                        Value = new Variable { Data = numberNode[0].NumberInt };
                        break;
                };

            }


            if (stringLiteralNode.Count > 0)
            {
                Value = new Variable { Data = stringLiteralNode[0].StringLiteral };
            }

            if (idNod.Count > 0)
            {
                var variable = new Variable { Name = idNod[0].Id };
                Variables.Add(idNod[0].Id, variable);
                Value = variable;
            }

            if (operatorNode.Count > 0)
            {
                _childOperator = operatorNode[0];
                foreach (var variable in _childOperator.Variables)
                {
                    Variables.Add(variable.Key, variable.Value);
                }
            }
        }
    }
}
