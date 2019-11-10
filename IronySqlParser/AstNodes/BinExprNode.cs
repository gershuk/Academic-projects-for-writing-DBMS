using System.Collections.Generic;
using System.Linq;

namespace IronySqlParser.AstNodes
{
    internal class BinExprNode : OperatorNode
    {
        private BinOp _binOp;
        private ExpressionNode _leftOperand;
        private ExpressionNode _rightOperand;

        public override dynamic Calc() =>

             _binOp switch
             {
                 BinOp.Plus => _leftOperand.Calc() + _rightOperand.Calc(),

                 BinOp.Minus => _leftOperand.Calc() - _rightOperand.Calc(),

                 BinOp.Multiply => _leftOperand.Calc() * _rightOperand.Calc(),

                 BinOp.Divide => _leftOperand.Calc() / _rightOperand.Calc(),

                 BinOp.Modulo => _leftOperand.Calc() % _rightOperand.Calc(),

                 BinOp.BitAnd => _leftOperand.Calc() & _rightOperand.Calc(),

                 BinOp.BitOr => _leftOperand.Calc() | _rightOperand.Calc(),

                 BinOp.ExclusiveOr => _leftOperand.Calc() ^ _rightOperand.Calc(),

                 BinOp.Equal => _leftOperand.Calc() == _rightOperand.Calc(),

                 BinOp.Greater => _leftOperand.Calc() > _rightOperand.Calc(),

                 BinOp.Less => _leftOperand.Calc() < _rightOperand.Calc(),

                 BinOp.EqualGreater => _leftOperand.Calc() >= _rightOperand.Calc(),

                 BinOp.EqualLess => _leftOperand.Calc() <= _rightOperand.Calc(),

                 BinOp.NotEqual => _leftOperand.Calc() != _rightOperand.Calc(),

                 BinOp.And => _leftOperand.Calc() && _rightOperand.Calc(),

                 BinOp.Or => _leftOperand.Calc() || _rightOperand.Calc()
             };


        public override void CollectInfoFromChild()
        {
            _binOp = FindFirstChildNodeByType<BinOpNode>().BinOp;
            var childNodes = ChildNodes.ToArray();
            _leftOperand = (ExpressionNode)childNodes[0];
            _rightOperand = (ExpressionNode)childNodes[2];

            Variables = new Dictionary<List<string>, Variable>();

            foreach (var variable in _leftOperand.Variables)
            {
                Variables.Add(variable.Key, variable.Value);
            }

            foreach (var variable in _rightOperand.Variables)
            {
                Variables.Add(variable.Key, variable.Value);
            }
        }
    }
}
