using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace IronySqlParser.Tests
{
    [TestClass]
    public class IsSequenceValidTests
    {   
        private SqlSequenceParser parser;


        [TestInitialize]
        public void TestInit() => parser = new SqlSequenceParser();


        [DataTestMethod]
        [DataRow("CREATE TABLE MyTable (id INT, address VARCHAR, name VARCHAR, age FLOAT)")]
        [DataRow("CREATE TABLE MyTable(id INT, address VARCHAR, name VARCHAR, age FLOAT)")]
        [DataRow("CREATE TABLE MyTable(id INT, address VARCHAR\n\n, name VARCHAR, age FLOAT)")]
        [DataRow("CREATE TABLE MyTable(id INT, address VARCHAR,     name VARCHAR, age FLOAT)")]
        [DataRow("     CREATE TABLE MyTable (id INT, address \n      \nVARCHAR, name VARCHAR)")]
        [DataRow("CREATE TABLE MyTable (id INT)")]
        [DataRow("CREATE TABLE MyTable_Word (id INT)")]
        [DataRow("CREATE TABLE MyTable.Word (id INT)")]
        [DataRow("CREATE TABLE MyTable_.Word (id INT)")]
        [DataRow("SHOW TABLE MyTable")]
        [DataRow("SHOW TABLE MyTable; SHOW TABLE MyTable;")]
        [DataRow("DROP TABLE tbl")]
        [DataRow("DROP TABLE TABLE")]
        public void TestIsValid_ReturnTrue(string query) => Assert.IsTrue(parser.IsSequenceValid(query));


        [DataTestMethod]
        [DataRow("")]
        [DataRow(";")]
        [DataRow("CREATE")]
        [DataRow("CREATE TABLE")]
        [DataRow("CREATE TABLE MyTable ()")]
        [DataRow("CREATE TABLE MyTable (id)")]
        [DataRow("CREATE TABLE MyTable; (id INT)")]
        [DataRow("CREATE TABLE .MyTable (id INT)")]
        [DataRow("CREATE TABLE MyTable (id INT, address VARCHAR, name VARCHARR)")]
        [DataRow("CREAT TABLE MyTable (id INT, address VARCHAR, name VARCHARR)")]
        [DataRow("CREATE TABLE MyTable.Table (id INT, address VARCHAR, name VARCHARR)")]
        [DataRow("CREATE TABLE MyTable Table (id INT, address VARCHAR, name VARCHARR)")]
        [DataRow("CREATE TABLE MyTable Table (id INT, address VARCHAR, name VARCHAR);;;")]
        [DataRow("CREATE TABLE MyTable Table (id INT, address VARCHAR, name VARCHAR).")]
        [DataRow("CREATE TABLE MyTable Table (id INT, address VARCHAR, name VARCHAR) name VARCHAR;")]
        [DataRow("SHOW TABLEE MyTable")]
        [DataRow("SHOW TABLE .MyTable;")]
        [DataRow("SHOW TABLE; MyTable;")]
        [DataRow("SHOW TABLE; MyTable;")]
        [DataRow("DROP TABE tbl;")]
        [DataRow("DROP TABLE")]
        public void IsValid_ReturnFalse(string query) => Assert.IsFalse(parser.IsSequenceValid(query));
    }

    [TestClass]
    public class LexicalTreeTests
    {
        private StringWriter sw;
        private SqlSequenceParser parser;


        [TestInitialize]
        public void TestInit()
        {
            parser = new SqlSequenceParser();
            sw = new StringWriter();
            Console.SetOut(sw);
            Console.SetError(sw);
        }


        [DataTestMethod]
        [DataRow(
            "DROP TABLE tbl;",
            "stmtList\r\n  DropTableStmt\r\n    drop (Keyword)\r\n    table (Keyword)\r\n    id\r\n      tbl (id_simple)\r\n"
        )]
        [DataRow(
            "     CREATE TABLE MyTable (id INT, address \n      \nVARCHAR, name VARCHAR)",
            "stmtList\r\n  CreateTableStmt\r\n    create (Keyword)\r\n    table (Keyword)\r\n    id\r\n      MyTable (id_simple)\r\n    fieldDefList\r\n      fieldDef\r\n        id\r\n          id (id_simple)\r\n        typeName\r\n          int (Keyword)\r\n        typeParams\r\n        constraintListOpt\r\n        nullSpecOpt\r\n      fieldDef\r\n        id\r\n          address (id_simple)\r\n        typeName\r\n          varchar (Keyword)\r\n        typeParams\r\n        constraintListOpt\r\n        nullSpecOpt\r\n      fieldDef\r\n        id\r\n          name (id_simple)\r\n        typeName\r\n          varchar (Keyword)\r\n        typeParams\r\n        constraintListOpt\r\n        nullSpecOpt\r\n"
        )]
        [DataRow(
            "SHOW TABLE tbl;",
            "stmtList\r\n  ShowTableStmt\r\n    show (Keyword)\r\n    table (Keyword)\r\n    id\r\n      tbl (id_simple)\r\n"
        )]
        [DataRow(
            "CREATE TABLE MyTable(id INT, address VARCHAR, name VARCHAR, age FLOAT);",
            "stmtList\r\n  CreateTableStmt\r\n    create (Keyword)\r\n    table (Keyword)\r\n    id\r\n      MyTable (id_simple)\r\n    fieldDefList\r\n      fieldDef\r\n        id\r\n          id (id_simple)\r\n        typeName\r\n          int (Keyword)\r\n        typeParams\r\n        constraintListOpt\r\n        nullSpecOpt\r\n      fieldDef\r\n        id\r\n          address (id_simple)\r\n        typeName\r\n          varchar (Keyword)\r\n        typeParams\r\n        constraintListOpt\r\n        nullSpecOpt\r\n      fieldDef\r\n        id\r\n          name (id_simple)\r\n        typeName\r\n          varchar (Keyword)\r\n        typeParams\r\n        constraintListOpt\r\n        nullSpecOpt\r\n      fieldDef\r\n        id\r\n          age (id_simple)\r\n        typeName\r\n          float (Keyword)\r\n        typeParams\r\n        constraintListOpt\r\n        nullSpecOpt\r\n"
        )]
        [DataRow(
            "CREATE TABLE MyTable_.Word (id INT)",
            "stmtList\r\n  CreateTableStmt\r\n    create (Keyword)\r\n    table (Keyword)\r\n    id\r\n      MyTable_ (id_simple)\r\n      Word (id_simple)\r\n    fieldDefList\r\n      fieldDef\r\n        id\r\n          id (id_simple)\r\n        typeName\r\n          int (Keyword)\r\n        typeParams\r\n        constraintListOpt\r\n        nullSpecOpt\r\n"
        )]
        [DataRow("CREAT TABLE mytable (id INT)", "")]
        public void BuildAndShowLexicalTree(string query, string result)
        {
            var root = parser.BuildLexicalTree(query).Root;
            parser.ShowLexicalTree(root, 0);
            Assert.AreEqual(EscapeIt(sw.ToString()), EscapeIt(result));
        }

        private static string EscapeIt(string value)
        {
            var builder = new StringBuilder();
            foreach (var cur in value)
            {
                switch (cur)
                {
                    case '\r':
                        builder.Append(@"\r");
                        break;
                    case '\n':
                        builder.Append(@"\n");
                        break;
                    default:
                        builder.Append(cur);
                        break;
                }
            }
            return builder.ToString();
        }
    }
}
