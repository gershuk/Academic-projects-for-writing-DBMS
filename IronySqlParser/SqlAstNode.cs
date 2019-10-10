using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Irony.Ast;
using Irony.Parsing;

namespace IronySqlParser
{
    public sealed class Token
    {
        internal Token(int column, int line, string text, object value)
        {
            Text = text;
            Value = value;
            Line = line;
            Column = column;
        }

        public int Column { get; private set; }
        public int Line { get; private set; }
        public string Text { get; private set; }
        public object Value { get; private set; }

        public override string ToString()
        {
            return Text;
        }
    }
    
    public interface ISqlNode
    {
        string NodeName { get; }
        ISqlNode Parent { get; }
        IEnumerable<ISqlNode> ChildNodes { get; }
        IEnumerable<Token> Tokens { get; }
    }

    interface ISqlChildNode : ISqlNode
    {
        void SetParent(ISqlNode node);
    }

    class SqlKeyNode : ISqlChildNode
    {
        internal SqlKeyNode(Token token)
        {
            this.token = token;
            Text = token.Text;
        }

        private readonly Token token;
        private ISqlNode parent;

        string ISqlNode.NodeName
        {
            get { return Text; }
        }

        ISqlNode ISqlNode.Parent
        {
            get { return parent; }
        }

        IEnumerable<ISqlNode> ISqlNode.ChildNodes
        {
            get { return new ISqlNode[0]; }
        }

        IEnumerable<Token> ISqlNode.Tokens
        {
            get { return new Token[] { token }; }
        }

        void ISqlChildNode.SetParent(ISqlNode node)
        {
            parent = node;
        }

        public string Text { get; private set; }
    }

    public class SqlNode : ISqlNode, IAstNodeInit
    {
        public SqlNode()
        {
            ChildNodes = new ReadOnlyCollection<ISqlNode>(new ISqlNode[0]);
            Tokens = new ReadOnlyCollection<Token>(new Token[0]);
        }

        protected ISqlNode Parent { get; private set; }

        protected string NodeName { get; private set; }

        protected IEnumerable<ISqlNode> ChildNodes { get; private set; }

        protected IEnumerable<Token> Tokens { get; private set; }

        void IAstNodeInit.Init(AstContext context, ParseTreeNode parseNode)
        {
            NodeName = parseNode.Term == null ? GetType().Name : parseNode.Term.Name;

            var tokens = new List<Token>();

            var iToken = parseNode.FindToken();
            if (iToken != null)
            {
                tokens.Add(new Token(iToken.Location.Column, iToken.Location.Line, iToken.Text, iToken.Value));
            }

            var childNodes = new List<ISqlNode>();

            foreach (var childNode in parseNode.ChildNodes)
            {
                ISqlNode child;
                if (childNode.Term is KeyTerm)
                {
                    var childIToken = childNode.FindToken();
                    child = new SqlKeyNode(new Token(childIToken.Location.Column, childIToken.Location.Line, childIToken.Text, childIToken.Value));
                }
                else
                {
                    child = (ISqlNode)childNode.AstNode;
                }

                child = OnChildNode(child);

                if (child != null)
                {
                    if (child is ISqlChildNode)
                        (child as ISqlChildNode).SetParent(this);

                    childNodes.Add(child);
                    tokens.AddRange(child.Tokens);
                }
            }

            ChildNodes = childNodes.ToArray();
            Tokens = tokens.ToArray();

            OnNodeInit();
        }

        string ISqlNode.NodeName
        {
            get { return NodeName; }
        }

        ISqlNode ISqlNode.Parent
        {
            get { return Parent; }
        }

        IEnumerable<ISqlNode> ISqlNode.ChildNodes
        {
            get { return ChildNodes; }
        }

        IEnumerable<Token> ISqlNode.Tokens
        {
            get { return Tokens; }
        }

        protected virtual void OnNodeInit()
        {
        }

        protected virtual ISqlNode OnChildNode(ISqlNode node)
        {
            return node;
        }
    }
}
