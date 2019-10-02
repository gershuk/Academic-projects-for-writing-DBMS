using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var id = new NonTerminal("id");
            var sqlSequence = new NonTerminal("sqlSequence");
            var createTableStmt = new NonTerminal("CreateTableStmt");
            var showTableStmt = new NonTerminal("ShowTableStmt");
            var alterStmt = new NonTerminal("alterStmt");
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


            //BNF Rules
            this.Root = stmtList;
            stmtLine.Rule = sqlSequence + semiOpt;
            semiOpt.Rule = Empty | ";";
            stmtList.Rule = MakePlusRule(stmtList, stmtLine);

            //ID
            id.Rule = MakePlusRule(id, dot, simpleId);

            sqlSequence.Rule = createTableStmt | alterStmt
                      | dropTableStmt | showTableStmt;
            //Create table
            createTableStmt.Rule = CREATE + TABLE + id + "(" + fieldDefList  + ")" ;
            fieldDefList.Rule = MakePlusRule(fieldDefList, comma, fieldDef);
            fieldDef.Rule = id + typeName + typeParamsOpt + constraintListOpt + nullSpecOpt;
            nullSpecOpt.Rule = NULL | NOT + NULL | Empty;
            typeName.Rule = ToTerm("BIT") | "DATE" | "TIME" | "TIMESTAMP" | "DECIMAL" | "REAL" | "FLOAT" | "SMALLINT" | "INTEGER"
                                         | "INTERVAL" | "CHARACTER" | "DATETIME" | "INT" | "DOUBLE" | "CHAR" | "NCHAR" | "VARCHAR"
                                         | "NVARCHAR" | "IMAGE" | "TEXT" | "NTEXT";
            typeParamsOpt.Rule = "(" + number + ")" | "(" + number + comma + number + ")" | Empty;

            constraintListOpt.Rule = MakeStarRule(constraintListOpt, constraintDef);
            constraintDef.Rule = (NOT + NULL) | (UNIQUE) | (PRIMARY + KEY) | ("Foreign" + KEY + "References" + id) | ("Default" + stringLiteral) | ("Index" + id);

            //Alter 
            alterStmt.Rule = ALTER + TABLE + id + alterCmd;
            alterCmd.Rule = ADD + COLUMN + fieldDefList + constraintListOpt
                          | ADD + constraintDef
                          | DROP + COLUMN + id
                          | DROP + CONSTRAINT + id;

            //Drop stmts
            dropTableStmt.Rule = DROP + TABLE + id;

            //Show table
            showTableStmt.Rule = SHOW + TABLE + id;

            MarkPunctuation(",", "(", ")");
            MarkPunctuation(asOpt, semiOpt);

            base.MarkTransient(sqlSequence, term, asOpt, aliasOpt, stmtLine, expression, unOp, tuple);
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

