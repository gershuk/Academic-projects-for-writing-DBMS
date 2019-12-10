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
            if (!variables.TryGetValue(_variableName,out var value))
            {
                throw new NullReferenceException();
            }

            Value = value;

            return _childOperator == null ? Value : _childOperator.Calc(variables);
        }

        public override void CollectInfoFromChild ()
        {
            VariablesNames = new List<Id>();

            var numberNode = FindAllChildNodesByType<NumberNode>();
            var stringLiteralNode = FindAllChildNodesByType<StringLiteralNode>();
            var idNod = FindAllChildNodesByType<IdNode>();
            var operatorNode = FindAllChildNodesByType<OperatorNode>();

            if (numberNode.Count > 0)
            {
                switch (numberNode[0].NumberType)
                {
                    case NumberType.Double:
                        Value = numberNode[0].NumberDouble;
                        break;
                    case NumberType.Int:
                        Value = numberNode[0].NumberInt;
                        break;
                };
            }

            if (stringLiteralNode.Count > 0)
            {
                Value = stringLiteralNode[0].StringLiteral;
            }

            if (idNod.Count > 0)
            {
                _variableName =new Id(idNod[0].Id);
                VariablesNames.Add(_variableName);
            }

            if (operatorNode.Count > 0)
            {
                _childOperator = operatorNode[0];
                GetAllValuesNamesFromNode(operatorNode[0]);
            }
        }
    }
}
