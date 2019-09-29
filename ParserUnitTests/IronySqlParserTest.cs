using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IronySqlParser;
using System.Collections.Generic;

namespace SqlParserUnitTest
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void CheckCreateTable()
        {
            var grammar = new SqlGrammar();
            var parser = new SqlSequenceParser();

            var goodSequences = new List<string>() { "CREATE TABLE Customers (Id INT,Age FLOAT, Name VARCHAR);" ,
                                                    "CREATE TABLE Customers (Id INT);",
                                                    "CREATE TABLE Customers.Microsoft (Id INT);"};
            foreach (var sequence in goodSequences)
            {
                Assert.AreEqual(parser.IsSequenceValid(sequence), true);
            }

            var badSequences = new List<string>() { "CREATE TABLE #&! (Id INT,Age FLOAT, Name VARCHAR);" ,
                                                    "CREATE TABLE Customers (Id);",
                                                    "CREATE TABLE Customers Microsoft (Id INT);",
                                                    "CREATE TABL Customers (Id INT);"};

            foreach (var sequence in badSequences)
            {
                Assert.AreEqual(parser.IsSequenceValid(sequence), false);
            }
        }

        [TestMethod]
        public void DropTable()
        {
            var grammar = new SqlGrammar();
            var parser = new SqlSequenceParser();

            var goodSequences = new List<string>() { "DROP TABLE table;", "DROP TABLE table.GGGGG;" };
            foreach (var sequence in goodSequences)
            {
                Assert.AreEqual(parser.IsSequenceValid(sequence), true);
            }

            var badSequences = new List<string>() { "DROP TABL table;", "DROPqqqw TABLE table.GGGGG;",
                                                    "DROP TABLE !232table;", "DROP TABLE table.G22$#&GGGG;",
                                                    "DROP TABLE table dfdasad; ", "DROP table.GGGGG;"};

            foreach (var sequence in badSequences)
            {
                Assert.AreEqual(parser.IsSequenceValid(sequence), false);
            }
        }
    }
}
