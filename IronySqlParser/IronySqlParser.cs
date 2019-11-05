﻿using System;

using Irony.Parsing;

using IronySqlParser.AstNodes;

namespace IronySqlParser
{
    public class SqlGrammar : Irony.Interpreter.InterpretedLanguageGrammar
    {
        public SqlGrammar() : base(false)
        {
            var comment = new CommentTerminal("comment", "/*", "*/");
            var lineComment = new CommentTerminal("line_comment", "--", "\n", "\r\n");
            NonGrammarTerminals.Add(comment);
            NonGrammarTerminals.Add(lineComment);

            var number = new NumberLiteral("number");
            number.AstConfig.NodeType = typeof(NumberNode);
            var stringLiteral = new StringLiteral("string", "'", StringOptions.AllowsDoubledQuote);
            stringLiteral.AstConfig.NodeType = typeof(StringLiteralNode);
            var simpleId = TerminalFactory.CreateSqlExtIdentifier(this, "id_simple");
            simpleId.AstConfig.NodeType = typeof(SimpleIdNode);

            var comma = ToTerm(",");
            var dot = ToTerm(".");
            var CREATE = ToTerm("CREATE");
            var SHOW = ToTerm("SHOW");
            var NULL = ToTerm("NULL");
            var NOT = ToTerm("NOT");
            var UNIQUE = ToTerm("UNIQUE");
            var TABLE = ToTerm("TABLE");
            var ALTER = ToTerm("ALTER");
            var ADD = ToTerm("ADD");
            var COLUMN = ToTerm("COLUMN");
            var DROP = ToTerm("DROP");
            var CONSTRAINT = ToTerm("CONSTRAINT");
            var KEY = ToTerm("KEY");
            var PRIMARY = ToTerm("PRIMARY");
            var SELECT = ToTerm("SELECT");
            var FROM = ToTerm("FROM");
            var AS = ToTerm("AS");
            var COUNT = ToTerm("COUNT");
            var BY = ToTerm("BY");
            var INTO = ToTerm("INTO");
            var ON = ToTerm("ON");
            var BEGIN_TRANSACTION = ToTerm("BEGIN TRANSACTION");
            var COMMIT = ToTerm("COMMIT");
            var ROLLBACK = ToTerm("ROLLBACK");
            var SET = ToTerm("SET");
            var UPDATE = ToTerm("UPDATE");
            var INSERT = ToTerm("INSERT");
            var VALUES = ToTerm("VALUES");
            var STAR = ToTerm("*");
            var ALL = ToTerm("ALL");
            var UNION = ToTerm("UNION");
            var INTERSECT = ToTerm("INTERSECT");
            var EXCEPT = ToTerm("EXCEPT");
            var JOIN = ToTerm("JOIN");


            var stmtList = new NonTerminal("stmtList", typeof(StmtListNode));
            var stmtLine = new NonTerminal("stmtLine", typeof(SqlNode));
            var semiOpt = new NonTerminal("semiOpt", typeof(SqlNode));
            var sqlCommand = new NonTerminal("sqlSequence", typeof(SqlCommandNode));

            var transaction = new NonTerminal("transaction", typeof(SqlNode));
            var transactionList = new NonTerminal("transactionList", typeof(SqlNode));
            var transactionBeginOp = new NonTerminal("transactionBeginOp", typeof(SqlNode));
            var transactionEndOp = new NonTerminal("transactionEndOp", typeof(SqlNode));
            var transactionName = new NonTerminal("transactionName", typeof(SqlNode));

            var id = new NonTerminal("id", typeof(IdNode));
            var createTableStmt = new NonTerminal("CreateTableStmt", typeof(CreateTableCommandNode));
            var showTableStmt = new NonTerminal("ShowTableStmt", typeof(ShowTableCommandNode));

            var dropTableStmt = new NonTerminal("DropTableStmt", typeof(DropTableCommandNode));
            var fieldDef = new NonTerminal("fieldDef", typeof(FieldDefNode));
            var fieldDefList = new NonTerminal("fieldDefList", typeof(FieldDefListNode));
            var nullSpecOpt = new NonTerminal("nullSpecOpt", typeof(NullSpectOptNode));
            var typeName = new NonTerminal("typeName", typeof(TypeNameNode));
            var typeParamsOpt = new NonTerminal("typeParams", typeof(TypeParamsOptNode));
            var constraintDef = new NonTerminal("constraintDef", typeof(ConstraintDefNode));
            var constraintListOpt = new NonTerminal("constraintListOpt", typeof(ConstraintListOptNodes));

            var alterStmt = new NonTerminal("AlterStmt", typeof(SqlNode));
            var alterCmd = new NonTerminal("alterCmd", typeof(SqlNode));
            var expression = new NonTerminal("expression", typeof(ExpressionNode));
            var asOpt = new NonTerminal("asOpt", typeof(SqlNode));
            var aliasOpt = new NonTerminal("aliasOpt", typeof(SqlNode));
            var tuple = new NonTerminal("tuple", typeof(SqlNode));
            var term = new NonTerminal("term", typeof(TermNode));
            var unOp = new NonTerminal("unOp", typeof(UnOpNode));
            var binOp = new NonTerminal("binOp", typeof(BinOpNode));

            var selectStmt = new NonTerminal("SelectStmt", typeof(SelectCommandNode));
            var expressionList = new NonTerminal("exprList", typeof(ExpressionListNode));
            var selRestrOpt = new NonTerminal("selRestrOpt", typeof(SqlNode));
            var selList = new NonTerminal("selList", typeof(SelListNode));
            var intoClauseOpt = new NonTerminal("intoClauseOpt", typeof(SqlNode));
            var fromClauseOpt = new NonTerminal("fromClauseOpt", typeof(FromClauseNode));
            var groupClauseOpt = new NonTerminal("groupClauseOpt", typeof(SqlNode));
            var havingClauseOpt = new NonTerminal("havingClauseOpt", typeof(SqlNode));
            var orderClauseOpt = new NonTerminal("orderClauseOpt", typeof(SqlNode));
            var whereClauseOpt = new NonTerminal("whereClauseOpt", typeof(WhereClauseNode));
            var columnItemList = new NonTerminal("columnItemList", typeof(ColumnItemListNode));
            var columnItem = new NonTerminal("columnItem", typeof(ColumnItemNode));
            var columnSource = new NonTerminal("columnSource", typeof(ColumnSourceNode));
            var idList = new NonTerminal("idList", typeof(IdListNode));
            var idlistPar = new NonTerminal("idlistPar", typeof(SqlNode));
            var aggregate = new NonTerminal("aggregate", typeof(SqlNode));
            var aggregateArg = new NonTerminal("aggregateArg", typeof(SqlNode));
            var aggregateName = new NonTerminal("aggregateName", typeof(SqlNode));

            var orderList = new NonTerminal("orderList", typeof(SqlNode));
            var orderMember = new NonTerminal("orderMember", typeof(SqlNode));
            var orderDirOpt = new NonTerminal("orderDirOpt", typeof(SqlNode));

            var unExpr = new NonTerminal("unExpr", typeof(UnExprNode));
            var binExpr = new NonTerminal("binExpr", typeof(BinExprNode));
            var funCall = new NonTerminal("funCall", typeof(SqlNode));
            var parSelectStmt = new NonTerminal("parSelectStmt", typeof(SqlNode));
            var betweenExpr = new NonTerminal("betweenExpr", typeof(SqlNode));
            var notOpt = new NonTerminal("notOpt", typeof(SqlNode));
            var funArgs = new NonTerminal("funArgs", typeof(SqlNode));
            var inStmt = new NonTerminal("inStmt", typeof(SqlNode));

            var updateStmt = new NonTerminal("UpdateStmt", typeof(UpdateNode));
            var insertStmt = new NonTerminal("InsertStmt", typeof(InsertCommandNode));
            var intoOpt = new NonTerminal("intoOpt", typeof(SqlNode));
            var insertDataList = new NonTerminal("insertDataList", typeof(InsertDataListNode));
            var insertObject = new NonTerminal("insertObject", typeof(InsertObjectNode));
            var insertData = new NonTerminal("InsertData", typeof(InsertDataNode));
            var assignList = new NonTerminal("assignList", typeof(AssignmentListNode));
            var assignment = new NonTerminal("assignment", typeof(AssignmentNode));
            var columnNames = new NonTerminal("columnNames", typeof(ColumnNamesNode));

            var expressionInBrackets = new NonTerminal("expressionBrackets", typeof(SqlNode));


            var idLink = new NonTerminal("idLink", typeof(SqlNode));
            var idLinkInBrackets = new NonTerminal("idLinkInBrackets", typeof(SqlNode));

            var joinChainOpt = new NonTerminal("joinChainOpt", typeof(JoinChainOptNode));
            var joinKindOpt = new NonTerminal("joinKindOpt", typeof(JoinKindOptNode));
            var joinStatement = new NonTerminal("joinStatement", typeof(JoinStatementNode));

            var unionChainOpt = new NonTerminal("unionChainOpt", typeof(SqlNode));
            var unionKindOpt = new NonTerminal("unionKindOpt", typeof(SqlNode));

            var intersectChainOpt = new NonTerminal("intersectChainOpt", typeof(SqlNode));
            var exceptChainOpt = new NonTerminal("exceptChainOpt", typeof(SqlNode));


            //BNF Rules
            this.Root = transactionList;
            transactionName.Rule = id | Empty;
            transactionBeginOp.Rule = BEGIN_TRANSACTION + transactionName;
            transactionEndOp.Rule = COMMIT | ROLLBACK;
            transaction.Rule = transactionBeginOp + stmtList + transactionEndOp | stmtLine;
            transactionList.Rule = MakePlusRule(transactionList, transaction);
            stmtList.Rule = MakePlusRule(stmtList, stmtLine);
            stmtLine.Rule = sqlCommand + semiOpt;
            sqlCommand.Rule = createTableStmt | alterStmt
                      | dropTableStmt | showTableStmt | selectStmt | updateStmt | insertStmt;
            semiOpt.Rule = Empty | ";";

            //ID link node
            idLink.Rule = id | joinChainOpt | unionChainOpt | intersectChainOpt | exceptChainOpt | selectStmt;
            idLinkInBrackets.Rule = "(" + idLink + ")" | idLink;

            //Join
            joinChainOpt.Rule = "(" + idLink + ")" + joinKindOpt + JOIN + "(" + idLink + ")" + ON + joinStatement;
            joinKindOpt.Rule = Empty | "INNER" | "LEFT" | "RIGHT";
            joinStatement.Rule = id + "=" + id;

            //Union
            unionChainOpt.Rule = idLinkInBrackets + UNION + unionKindOpt + idLinkInBrackets;
            unionKindOpt.Rule = Empty | ALL;

            //Intersect
            intersectChainOpt.Rule = idLinkInBrackets + INTERSECT + idLinkInBrackets;

            //Except
            exceptChainOpt.Rule = idLinkInBrackets + EXCEPT + idLinkInBrackets;

            //ID
            id.Rule = MakePlusRule(id, dot, simpleId);
            idlistPar.Rule = "(" + idList + ")";
            idList.Rule = MakePlusRule(idList, comma, id);

            //Create table
            createTableStmt.Rule = CREATE + TABLE + id + "(" + fieldDefList + ")";
            fieldDefList.Rule = MakePlusRule(fieldDefList, comma, fieldDef);
            fieldDef.Rule = id + typeName + typeParamsOpt + constraintListOpt + nullSpecOpt;
            nullSpecOpt.Rule = NULL | NOT + NULL | Empty;
            typeName.Rule = ToTerm("DATETIME") | "INT" | "DOUBLE" | "CHAR" | "NCHAR" | "VARCHAR" | "NVARCHAR"
                                   | "IMAGE" | "TEXT" | "NTEXT";

            typeParamsOpt.Rule = "(" + number + ")" | "(" + number + comma + number + ")" | Empty;

            constraintListOpt.Rule = MakeStarRule(constraintListOpt, constraintDef);
            constraintDef.Rule = (UNIQUE) | (PRIMARY + KEY) | ("FOREIGN" + KEY + "REFERENCES" + id) | ("DEFAULT" + stringLiteral) | ("INDEX" + id);

            //Alter 
            alterStmt.Rule = ALTER + TABLE + id + alterCmd;
            alterCmd.Rule = ADD + simpleId + "(" + fieldDefList + ")" | DROP + COLUMN + id
                          | ALTER + COLUMN + id + typeName + (ADD + CONSTRAINT + constraintListOpt
                          | DROP + CONSTRAINT + constraintListOpt);

            //Drop stmts
            dropTableStmt.Rule = DROP + TABLE + id;

            //Show table
            showTableStmt.Rule = SHOW + TABLE + id;

            //Select stmt
            selectStmt.Rule = SELECT + selRestrOpt + selList + intoClauseOpt + fromClauseOpt + whereClauseOpt +
                              groupClauseOpt + havingClauseOpt + orderClauseOpt;
            selRestrOpt.Rule = Empty | ALL | "DISTINCT";
            selList.Rule = columnItemList | STAR;
            columnItemList.Rule = MakePlusRule(columnItemList, comma, columnItem);
            columnItem.Rule = columnSource + aliasOpt;
            aliasOpt.Rule = Empty | asOpt + id;
            asOpt.Rule = Empty | AS;
            columnSource.Rule = aggregate | id;
            aggregate.Rule = aggregateName + "(" + aggregateArg + ")";
            aggregateArg.Rule = expression | STAR;
            aggregateName.Rule = COUNT | "Avg" | "Min" | "Max" | "StDev" | "StDevP" | "Sum" | "Var" | "VarP";
            intoClauseOpt.Rule = Empty | INTO + id;
            fromClauseOpt.Rule = Empty | FROM + idLinkInBrackets;
            whereClauseOpt.Rule = Empty | "WHERE" + expression;
            groupClauseOpt.Rule = Empty | "GROUP" + BY + idList;
            havingClauseOpt.Rule = Empty | "HAVING" + expression;
            orderClauseOpt.Rule = Empty | "ORDER" + BY + orderList;
            orderList.Rule = MakePlusRule(orderList, comma, orderMember);
            orderMember.Rule = id + orderDirOpt;
            orderDirOpt.Rule = Empty | "ASC" | "DESC";

            //Insert stmt
            insertStmt.Rule = INSERT + intoOpt + id + columnNames + insertData;
            insertDataList.Rule = MakePlusRule(insertDataList, comma, insertObject);
            insertObject.Rule = "(" + expressionList + ")";
            columnNames.Rule = idlistPar | Empty;
            insertData.Rule = selectStmt | VALUES + insertDataList;
            intoOpt.Rule = Empty | INTO; //Into is optional in MSSQL

            //Update stmt
            updateStmt.Rule = UPDATE + id + SET + assignList + whereClauseOpt;
            assignList.Rule = MakePlusRule(assignList, comma, assignment);
            assignment.Rule = id + "=" + expression;


            //Expression
            expressionList.Rule = MakePlusRule(expressionList, comma, expressionInBrackets);
            expressionInBrackets.Rule = "(" + expression + ")" | expression;
            expression.Rule = term | unExpr | binExpr;// Add betweenExpr
            term.Rule = id | stringLiteral | number; //| funCall | tuple | parSelectStmt;// | inStmt;
            tuple.Rule = "(" + expressionList + ")";
            parSelectStmt.Rule = "(" + selectStmt + ")";
            unExpr.Rule = unOp + expressionInBrackets;
            unOp.Rule = NOT | "+" | "-" | "~";
            binExpr.Rule = expressionInBrackets + binOp + expressionInBrackets;
            binOp.Rule = ToTerm("+") | "-" | "*" | "/" | "%" | "&" | "|" | "^"
                       | "=" | ">" | "<" | ">=" | "<=" | "<>" | "!=" | "!<" | "!>"
                       | "AND" | "OR" | "LIKE" | NOT + "LIKE" | "IN" | NOT + "IN";
            betweenExpr.Rule = expression + notOpt + "BETWEEN" + expression + "AND" + expression;
            notOpt.Rule = Empty | NOT;
            funCall.Rule = id + "(" + funArgs + ")";
            funArgs.Rule = selectStmt | expressionList;
            inStmt.Rule = expression + "IN" + "(" + expressionList + ")";

            //Operators
            RegisterOperators(10, "*", "/", "%");
            RegisterOperators(10, JOIN, UNION, INTERSECT, EXCEPT);
            RegisterOperators(9, "+", "-");
            RegisterOperators(8, "=", ">", "<", ">=", "<=", "<>", "!=", "!<", "!>", "LIKE", "IN");
            RegisterOperators(7, "^", "&", "|");
            RegisterOperators(6, NOT);
            RegisterOperators(5, "AND");
            RegisterOperators(4, "OR");

            MarkPunctuation(",", "(", ")");
            MarkPunctuation(asOpt, semiOpt);

            MarkTransient(sqlCommand, term, asOpt, aliasOpt, stmtLine, tuple, expressionInBrackets, idlistPar, idLinkInBrackets, unionKindOpt);
            //LanguageFlags = LanguageFlags.CreateAst;
            binOp.SetFlag(TermFlags.InheritPrecedence);
        }
    }

    public class SqlSequenceParser
    {
        private readonly SqlGrammar _sqlGrammar;
        private readonly LanguageData language;
        private readonly Parser parser;

        public SqlSequenceParser()
        {
            _sqlGrammar = new SqlGrammar();
            language = new LanguageData(_sqlGrammar);
            parser = new Parser(language);
        }

        public bool IsSequenceValid(string sequence)
        {
            var parseTree = parser.Parse(sequence);
            var root = parseTree.Root;
            return root != null;
        }

        public ParseTree BuildLexicalTree(string sequence) => parser.Parse(sequence);

        public void ShowLexicalTree(ParseTreeNode node, int level = 0)
        {
            if (node != null)
            {
                for (var i = 0; i < level; i++)
                {
                    Console.Write("  ");
                }

                Console.WriteLine(node);

                foreach (var child in node.ChildNodes)
                {
                    ShowLexicalTree(child, level + 1);
                }
            }
        }
    }
}

