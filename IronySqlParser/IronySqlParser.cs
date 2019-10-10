using System;

using Irony.Parsing;

namespace IronySqlParser
{
    public class SqlGrammar : Grammar
    {
        public SqlGrammar() : base(false)
        {
            var comment = new CommentTerminal("comment", "/*", "*/");
            var lineComment = new CommentTerminal("line_comment", "--", "\n", "\r\n");
            NonGrammarTerminals.Add(comment);
            NonGrammarTerminals.Add(lineComment);

            var number = new NumberLiteral("number");
            var stringLiteral = new StringLiteral("string", "'", StringOptions.AllowsDoubledQuote);
            var simpleId = TerminalFactory.CreateSqlExtIdentifier(this, "id_simple");
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
            var JOIN = ToTerm("JOIN");
            var BY = ToTerm("BY");
            var INTO = ToTerm("INTO");
            var ON = ToTerm("ON");
            var BEGIN_TRANSACTION = ToTerm("BEGIN TRANSACTION");
            var COMMIT = ToTerm("COMMIT");
            var SET = ToTerm("SET");
            var UPDATE = ToTerm("UPDATE");
            var INSERT = ToTerm("INSERT");
            var VALUES = ToTerm("VALUES");

            var id = new NonTerminal("id");
            var sqlCommand = new NonTerminal("sqlSequence");
            var createTableStmt = new NonTerminal("CreateTableStmt",typeof(SqlNode));
            var showTableStmt = new NonTerminal("ShowTableStmt");
            var alterStmt = new NonTerminal("AlterStmt");
            var dropTableStmt = new NonTerminal("DropTableStmt");
            var fieldDef = new NonTerminal("fieldDef");
            var fieldDefList = new NonTerminal("fieldDefList");
            var nullSpecOpt = new NonTerminal("nullSpecOpt");
            var typeName = new NonTerminal("typeName");
            var typeParamsOpt = new NonTerminal("typeParams");
            var constraintDef = new NonTerminal("constraintDef");
            var constraintListOpt = new NonTerminal("constraintListOpt");
            var alterCmd = new NonTerminal("alterCmd");
            var expression = new NonTerminal("expression");
            var asOpt = new NonTerminal("asOpt");
            var aliasOpt = new NonTerminal("aliasOpt");
            var tuple = new NonTerminal("tuple");
            var term = new NonTerminal("term");
            var unOp = new NonTerminal("unOp");
            var binOp = new NonTerminal("binOp");
            var stmtLine = new NonTerminal("stmtLine");
            var semiOpt = new NonTerminal("semiOpt");
            var stmtList = new NonTerminal("stmtList");

            var selectStmt = new NonTerminal("SelectStmt");
            var exprList = new NonTerminal("exprList");
            var selRestrOpt = new NonTerminal("selRestrOpt");
            var selList = new NonTerminal("selList");
            var intoClauseOpt = new NonTerminal("intoClauseOpt");
            var fromClauseOpt = new NonTerminal("fromClauseOpt");
            var groupClauseOpt = new NonTerminal("groupClauseOpt");
            var havingClauseOpt = new NonTerminal("havingClauseOpt");
            var orderClauseOpt = new NonTerminal("orderClauseOpt");
            var whereClauseOpt = new NonTerminal("whereClauseOpt");
            var columnItemList = new NonTerminal("columnItemList");
            var columnItem = new NonTerminal("columnItem");
            var columnSource = new NonTerminal("columnSource");
            var idList = new NonTerminal("idList");
            var idlistPar = new NonTerminal("idlistPar");
            var aggregate = new NonTerminal("aggregate");
            var aggregateArg = new NonTerminal("aggregateArg");
            var aggregateName = new NonTerminal("aggregateName");
            var joinChainOpt = new NonTerminal("joinChainOpt");
            var joinKindOpt = new NonTerminal("joinKindOpt");
            var orderList = new NonTerminal("orderList");
            var orderMember = new NonTerminal("orderMember");
            var orderDirOpt = new NonTerminal("orderDirOpt");

            var unExpr = new NonTerminal("unExpr");
            var binExpr = new NonTerminal("binExpr");
            var funCall = new NonTerminal("funCall");
            var parSelectStmt = new NonTerminal("parSelectStmt");
            var betweenExpr = new NonTerminal("betweenExpr");
            var notOpt = new NonTerminal("notOpt");
            var funArgs = new NonTerminal("funArgs");
            var inStmt = new NonTerminal("inStmt");

            var updateStmt = new NonTerminal("UpdateStmt");
            var insertStmt = new NonTerminal("InsertStmt");
            var intoOpt = new NonTerminal("intoOpt");
            var insertData = new NonTerminal("InsertData");
            var assignList = new NonTerminal("assignList");
            var assignment = new NonTerminal("assignment");

            var sqlSequence = new NonTerminal("sqlSequence");
            var columnNames = new NonTerminal("columnNames");

            sqlCommand.Rule = createTableStmt | alterStmt
                      | dropTableStmt | showTableStmt | selectStmt | updateStmt | insertStmt;

            //BNF Rules
            this.Root = stmtList;
            stmtLine.Rule = sqlCommand + semiOpt;
            semiOpt.Rule = Empty | ";";
            stmtList.Rule = MakePlusRule(stmtList, stmtLine);

            //ID
            id.Rule = MakePlusRule(id, dot, simpleId);
            idlistPar.Rule = "(" + idList + ")";
            idList.Rule = MakePlusRule(idList, comma, id);

            //Create table
            createTableStmt.Rule = CREATE + TABLE + id + "(" + fieldDefList  + ")" ;
            fieldDefList.Rule = MakePlusRule(fieldDefList, comma, fieldDef);
            fieldDef.Rule = id + typeName + typeParamsOpt + constraintListOpt + nullSpecOpt;
            nullSpecOpt.Rule = NULL | NOT + NULL | Empty;
            typeName.Rule = ToTerm("DATETIME") | "INT" | "DOUBLE" | "CHAR" | "NCHAR" | "VARCHAR" | "NVARCHAR"
                                   | "IMAGE" | "TEXT" | "NTEXT"; ;

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
            selRestrOpt.Rule = Empty | "ALL" | "DISTINCT";
            selList.Rule = columnItemList | "*";
            columnItemList.Rule = MakePlusRule(columnItemList, comma, columnItem);
            columnItem.Rule = columnSource + aliasOpt;
            aliasOpt.Rule = Empty | asOpt + id;
            asOpt.Rule = Empty | AS;
            columnSource.Rule = aggregate | id;
            aggregate.Rule = aggregateName + "(" + aggregateArg + ")";
            aggregateArg.Rule = expression | "*";
            aggregateName.Rule = COUNT | "Avg" | "Min" | "Max" | "StDev" | "StDevP" | "Sum" | "Var" | "VarP";
            intoClauseOpt.Rule = Empty | INTO + id;
            fromClauseOpt.Rule = Empty | FROM + idList + joinChainOpt;
            joinChainOpt.Rule = Empty | joinKindOpt + JOIN + idList + ON + id + "=" + id;
            joinKindOpt.Rule = Empty | "INNER" | "LEFT" | "RIGHT";
            whereClauseOpt.Rule = Empty | "WHERE" + expression;
            groupClauseOpt.Rule = Empty | "GROUP" + BY + idList;
            havingClauseOpt.Rule = Empty | "HAVING" + expression;
            orderClauseOpt.Rule = Empty | "ORDER" + BY + orderList;
            orderList.Rule = MakePlusRule(orderList, comma, orderMember);
            orderMember.Rule = id + orderDirOpt;
            orderDirOpt.Rule = Empty | "ASC" | "DESC";

            //Insert stmt
            insertStmt.Rule = INSERT + intoOpt + id + columnNames + insertData;
            columnNames.Rule = idlistPar | Empty;
            insertData.Rule = selectStmt | VALUES + "(" + exprList + ")";
            intoOpt.Rule = Empty | INTO; //Into is optional in MSSQL

            //Update stmt
            updateStmt.Rule = UPDATE + id + SET + assignList + whereClauseOpt;
            assignList.Rule = MakePlusRule(assignList, comma, assignment);
            assignment.Rule = id + "=" + expression;


            //Expression
            exprList.Rule = MakePlusRule(exprList, comma, expression);
            expression.Rule = term | unExpr | binExpr;// Add betweenExpr
            term.Rule = id | stringLiteral | number | funCall | tuple | parSelectStmt;// | inStmt;
            tuple.Rule = "(" + exprList + ")";
            parSelectStmt.Rule = "(" + selectStmt + ")";
            unExpr.Rule = unOp + term;
            unOp.Rule = NOT | "+" | "-" | "~";
            binExpr.Rule = expression + binOp + expression;
            binOp.Rule = ToTerm("+") | "-" | "*" | "/" | "%" | "&" | "|" | "^" 
                       | "=" | ">" | "<" | ">=" | "<=" | "<>" | "!=" | "!<" | "!>"
                       | "AND" | "OR" | "LIKE" | NOT + "LIKE" | "IN" | NOT + "IN";
            betweenExpr.Rule = expression + notOpt + "BETWEEN" + expression + "AND" + expression;
            notOpt.Rule = Empty | NOT;
            funCall.Rule = id + "(" + funArgs + ")";
            funArgs.Rule = selectStmt | exprList;
            inStmt.Rule = expression + "IN" + "(" + exprList + ")";

            //Operators
            RegisterOperators(10, "*", "/", "%");
            RegisterOperators(9, "+", "-");
            RegisterOperators(8, "=", ">", "<", ">=", "<=", "<>", "!=", "!<", "!>", "LIKE", "IN");
            RegisterOperators(7, "^", "&", "|");
            RegisterOperators(6, NOT);
            RegisterOperators(5, "AND");
            RegisterOperators(4, "OR");

            MarkPunctuation(",", "(", ")");
            MarkPunctuation(asOpt, semiOpt);

            base.MarkTransient(sqlCommand, term, asOpt, aliasOpt, stmtLine, expression, unOp, tuple);
            binOp.SetFlag(TermFlags.InheritPrecedence);
        }
    }

    public class SqlSequenceParser
    {
        private SqlGrammar _sqlGrammar;
        private LanguageData language;
        private Parser parser;

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

        public void ShowLexicalTree(ParseTreeNode node, int level)
        {
            if (node != null) {
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

