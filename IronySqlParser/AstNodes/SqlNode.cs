using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Irony.Ast;
using Irony.Parsing;

using TransactionManagement;

namespace IronySqlParser.AstNodes
{
    public sealed class Token
    {
        public int Column { get; private set; }
        public int Line { get; private set; }
        public string Text { get; private set; }
        public object Value { get; private set; }

        public Token(int column, int line, string text, object value)
        {
            Text = text;
            Value = value;
            Line = line;
            Column = column;
        }

        public override string ToString() => Text;
    }

    public interface ISqlNode
    {
        string NodeName { get; }
        ISqlNode Parent { get; }
        IEnumerable<ISqlNode> ChildNodes { get; }
        IEnumerable<Token> Tokens { get; }

        public object Accept(ISqlNodeExecutor sqlNodeVisitor);
    }

    public interface ISqlChildNode : ISqlNode
    {
        void SetParent(ISqlNode node);
    }

    public interface ISqlNodeExecutor
    {
        public object ExecuteSqlNode(SqlNode node);
        public object ExecuteSqlNode(CreateTableCommandNode node);
        public object ExecuteSqlNode(DropTableCommandNode node);
        public object ExecuteSqlNode(ShowTableCommandNode node);
        public object ExecuteSqlNode(InsertCommandNode node);
        public object ExecuteSqlNode(DeleteCommandNode node);
        public object ExecuteSqlNode(SelectCommandNode node);
        public object ExecuteSqlNode(UpdateCommandNode node);
        public object ExecuteSqlNode(JoinChainOptNode node);
        public object ExecuteSqlNode(UnionChainOptNode node);
        public object ExecuteSqlNode(IntersectChainOptNode node);
        public object ExecuteSqlNode(ExceptChainOptNode node);
    }

    public class SqlKeyNode : ISqlChildNode
    {
        private readonly Token token;
        private ISqlNode parent;

        public SqlKeyNode(Token token)
        {
            this.token = token;
            Text = token.Text;
        }

        string ISqlNode.NodeName => Text;
        ISqlNode ISqlNode.Parent => parent;
        IEnumerable<ISqlNode> ISqlNode.ChildNodes => new ISqlNode[0];
        IEnumerable<Token> ISqlNode.Tokens => new Token[] { token };
        void ISqlChildNode.SetParent(ISqlNode node) => parent = node;
        public string Text { get; private set; }
        public object Accept(ISqlNodeExecutor sqlNodeVisitor) => null;
    }

    public class SqlNode : ISqlNode, IAstNodeInit
    {
        public IEnumerable<Token> Tokens { get; private set; }
        public List<SqlCommandNode> ExecuteCommnadsForNode { get; private set; }

        protected ISqlNode Parent { get; private set; }
        protected string NodeName { get; private set; }
        protected IEnumerable<ISqlNode> ChildNodes { get; private set; }

        public virtual void CollectInfoFromChild()
        { }

        protected virtual List<T> FindAllChildNodesByType<T>()
        {
            var children = new List<T>();

            foreach (var child in ChildNodes)
            {
                if (child is T)
                {
                    children.Add((T)child);
                }
            }

            return children;
        }

        public SqlNode()
        {
            ChildNodes = new ReadOnlyCollection<ISqlNode>(new ISqlNode[0]);
            Tokens = new ReadOnlyCollection<Token>(new Token[0]);
        }

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
                    {
                        (child as ISqlChildNode).SetParent(this);
                    }

                    childNodes.Add(child);
                    tokens.AddRange(child.Tokens);
                }
            }

            ChildNodes = childNodes.ToArray();
            Tokens = tokens.ToArray();

            CollectInfoFromChild();

            CollectAllCommands();

            OnNodeInit();
        }

        string ISqlNode.NodeName => NodeName;

        ISqlNode ISqlNode.Parent => Parent;

        IEnumerable<ISqlNode> ISqlNode.ChildNodes => ChildNodes;

        IEnumerable<Token> ISqlNode.Tokens => Tokens;

        protected virtual void OnNodeInit()
        { }

        protected virtual ISqlNode OnChildNode(ISqlNode node) => node;

        protected static T ParseEnum<T>(string value) => (T)Enum.Parse(typeof(T), value, true);


        protected virtual T FindFirstChildNodeByType<T>()
        {
            foreach (var child in ChildNodes)
            {
                if (child is T)
                {
                    return (T)child;
                }
            }

            return default;
        }

        protected void CollectAllCommands()
        {
            ExecuteCommnadsForNode = new List<SqlCommandNode>();

            if (ChildNodes != null)
            {
                foreach (var child in ChildNodes)
                {
                    if (child is SqlNode sqlNode)
                    {
                        ExecuteCommnadsForNode.AddRange(sqlNode.ExecuteCommnadsForNode);
                    }
                }
            }

            if (this is SqlCommandNode thisCommand)
            {
                ExecuteCommnadsForNode.Add(thisCommand);
            }
        }

        public object Accept(ISqlNodeExecutor sqlNodeVisitor) => sqlNodeVisitor.ExecuteSqlNode(this);
    }

    public abstract class SqlCommandNode : SqlNode
    {
        public List<string> ReturnedTableName { get; private set; } = new List<string>();

        public abstract List<TableLock> GetTableLocks();

        public void SetReturnedTableName(List<string> name) => ReturnedTableName.AddRange(name);
    }
}
