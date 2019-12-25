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

        public override dynamic Calc (Dictionary<string, dynamic> variables)
        {
            if (!_wasСalculated || !ConstOnly)
            {
                _cachedValue = _binOp switch
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
            }

            _wasСalculated = true;
            return _cachedValue;
        }

        public override void CollectDataFromChildren ()
        {
            _binOp = FindFirstChildNodeByType<BinOpNode>().BinOp;
            var childNodes = ChildNodes.ToArray();
            _leftOperand = (ExpressionNode)childNodes[0];
            _rightOperand = (ExpressionNode)childNodes[2];

            ConstOnly = _leftOperand.ConstOnly && _rightOperand.ConstOnly;

            GetAllValuesNamesFromNode(_leftOperand);
            GetAllValuesNamesFromNode(_rightOperand);

            if ((_leftOperand.VariablesNames.Count + _rightOperand.VariablesNames.Count == 1) && (_leftOperand.IsCompressed || _rightOperand.IsCompressed))
            {
                string variableName = default;
                dynamic constData = default;
                var left = false;

                if (_leftOperand.VariablesNames.Count == 1)
                {
                    variableName = _leftOperand.VariablesNames[0];
                    constData = _rightOperand.Calc(new Dictionary<string, dynamic>());
                    left = true;
                }
                else
                {
                    variableName = _rightOperand.VariablesNames[0];
                    constData = _leftOperand.Calc(new Dictionary<string, dynamic>());
                    left = false;
                }

                IsCompressed = true;
                switch (_binOp)
                {
                    case BinOp.Equal:
                        AddVariableBorder(variableName, new VariableBorder(constData, constData, false, false));
                        break;
                    case BinOp.Greater:
                        if (left)
                        {
                            AddVariableBorder(variableName, new VariableBorder(constData, null, true, true));
                        }
                        else
                        {
                            AddVariableBorder(variableName, new VariableBorder(null, constData, true, true));
                        }
                        break;
                    case BinOp.Less:
                        if (left)
                        {
                            AddVariableBorder(variableName, new VariableBorder(null, constData, true, true));
                        }
                        else
                        {
                            AddVariableBorder(variableName, new VariableBorder(constData, null, true, true));
                        }
                        break;
                    case BinOp.EqualGreater:
                        if (left)
                        {
                            AddVariableBorder(variableName, new VariableBorder(constData, null, false, false));
                        }
                        else
                        {
                            AddVariableBorder(variableName, new VariableBorder(null, constData, false, false));
                        }
                        break;
                    case BinOp.EqualLess:
                        if (left)
                        {
                            AddVariableBorder(variableName, new VariableBorder(null, constData, false, false));
                        }
                        else
                        {
                            AddVariableBorder(variableName, new VariableBorder(constData, null, false, false));
                        }
                        break;
                    default:
                        IsCompressed = false;
                        break;
                }
            }
            else
            {
                IsCompressed = false;
            }

            if (_binOp == BinOp.And && _leftOperand.IsCompressed && _rightOperand.IsCompressed)
            {
                foreach (var variableBorder in _leftOperand.VariablesBorder)
                {
                    AddVariableBorder(variableBorder.Key, (VariableBorder)variableBorder.Value.Clone());
                }

                foreach (var variableBorder in _rightOperand.VariablesBorder)
                {
                    AddVariableBorder(variableBorder.Key, (VariableBorder)variableBorder.Value.Clone());
                }

                IsCompressed = true;
            }
        }
    }
}
