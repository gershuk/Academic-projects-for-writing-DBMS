using System.Collections.Generic;
using Irony.Parsing;

namespace SunflowerDB
{
    public class ExpressionBuilder
    {
        public Dictionary<string, DataBaseTypeExpression> VariablesDictionary { get; set; }
        public ExpressionNode Root { get; set; }

        public ExpressionBuilder(ParseTreeNode node)
        {
            var childs = node.ChildNodes;
            

            if (childs[0].Term.Name == "id")
            {
                var operandName = EngineCommander.BuildNameFromId(childs[0]);
                var operand = new DataBaseTypeExpression();
                VariablesDictionary.Add(operandName, operand);
            }

            if (childs[2].Term.Name == "id")
            {
                var operandName = EngineCommander.BuildNameFromId(childs[2]);
                var operand = new DataBaseTypeExpression();
                VariablesDictionary.Add(operandName,operand);
            }
        }

        public abstract class ExpressionNode
        {
            protected dynamic LeftExpNode { get; set; }
            protected dynamic RightExpNode { get; set; }
            protected abstract dynamic Value();
        }

        protected class SumExpression : ExpressionNode
        {
            protected override object Value() => LeftExpNode.Value() + RightExpNode.Value();
        }

        protected class SubtractionExpression : ExpressionNode
        {
            protected override object Value() => LeftExpNode.Value() - RightExpNode.Value();
        }

        protected class MultiplicationExpression : ExpressionNode
        {
            protected override object Value() => LeftExpNode.Value() * RightExpNode.Value();
        }

        protected class DivisionExpression : ExpressionNode
        {
            protected override object Value() => LeftExpNode.Value() / RightExpNode.Value();
        }

        protected class GreateEqualExpression : ExpressionNode
        {
            protected override object Value() => LeftExpNode.Value() >= RightExpNode.Value();
        }

        protected class LessEqualsExpression : ExpressionNode
        {
            protected override object Value() => LeftExpNode.Value() <= RightExpNode.Value();
        }

        protected class GreateExpression : ExpressionNode
        {
            protected override object Value() => LeftExpNode.Value() > RightExpNode.Value();
        }

        protected class LessExpression : ExpressionNode
        {
            protected override object Value() => LeftExpNode.Value() < RightExpNode.Value();
        }

        protected class EqualExpression : ExpressionNode
        {
            protected override object Value() => LeftExpNode.Value() == RightExpNode.Value();
        }

        public class DataBaseTypeExpression : ExpressionNode
        {
            public DataBaseTypeExpression()
            {
            }

            protected DataBaseTypeExpression(dynamic value)
            {
                LeftExpNode = value;
            }

            protected override dynamic Value() => LeftExpNode;
        }
    }
}
