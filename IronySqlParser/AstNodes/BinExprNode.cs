using System;
using System.Collections.Generic;
using System.Linq;
using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class BinExprNode : OperatorNode
    {
        private BinOp _binOp;
        private ExpressionNode _leftOperand;
        private ExpressionNode _rightOperand;

        public override dynamic Calc (Dictionary<Id, dynamic> variables) =>

             _binOp switch
             {
                 BinOp.Plus => _leftOperand.Calc(variables) + _rightOperand.Calc(variables),

                 BinOp.Minus => _leftOperand.Calc(variables) - _rightOperand.Calc(variables),

                 BinOp.Multiply => _leftOperand.Calc(variables) * _rightOperand.Calc(variables),

                 BinOp.Divide => _leftOperand.Calc(variables) / _rightOperand.Calc(variables),

                 BinOp.Modulo => _leftOperand.Calc(variables) % _rightOperand.Calc(variables),

                 BinOp.BitAnd => _leftOperand.Calc(variables) & _rightOperand.Calc(variables),

                 BinOp.BitOr => _leftOperand.Calc(variables) | _rightOperand.Calc(variables),

                 BinOp.ExclusiveOr => _leftOperand.Calc(variables) ^ _rightOperand.Calc(variables),

                 BinOp.Equal => _leftOperand.Calc(variables) == _rightOperand.Calc(variables),

                 BinOp.Greater => _leftOperand.Calc(variables) > _rightOperand.Calc(variables),

                 BinOp.Less => _leftOperand.Calc(variables) < _rightOperand.Calc(variables),

                 BinOp.EqualGreater => _leftOperand.Calc(variables) >= _rightOperand.Calc(variables),

                 BinOp.EqualLess => _leftOperand.Calc(variables) <= _rightOperand.Calc(variables),

                 BinOp.NotEqual => _leftOperand.Calc(variables) != _rightOperand.Calc(variables),

                 BinOp.And => _leftOperand.Calc(variables) && _rightOperand.Calc(variables),

                 BinOp.Or => _leftOperand.Calc(variables) || _rightOperand.Calc(variables),

                 _ => throw new NotImplementedException()
             };


        public override void CollectInfoFromChild ()
        {
            _binOp = FindFirstChildNodeByType<BinOpNode>().BinOp;
            var childNodes = ChildNodes.ToArray();
            _leftOperand = (ExpressionNode)childNodes[0];
            _rightOperand = (ExpressionNode)childNodes[2];

            GetAllValuesNamesFromNode(_leftOperand);
            GetAllValuesNamesFromNode(_rightOperand);
        }
    }
}
