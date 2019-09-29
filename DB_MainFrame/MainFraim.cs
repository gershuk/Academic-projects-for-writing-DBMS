using System;
using System.Collections.Generic;
using IronySqlParser;
using DataBaseEngine;
using Irony.Parsing;
using System.Threading.Tasks;

namespace DB_MainFrame
{
    sealed class MainFrame
    {
        public MainFrame(SqlSequenceParser sqlParser, SimpleDataBaseEngine baseEngine, Queue<ParseTreeNode> sqlCommands)
        {
            SqlParser = sqlParser ?? throw new ArgumentNullException(nameof(sqlParser));
            BaseEngine = baseEngine ?? throw new ArgumentNullException(nameof(baseEngine));
            SqlCommands = sqlCommands ?? throw new ArgumentNullException(nameof(sqlCommands));
        }

        public SqlSequenceParser SqlParser { get; set; }
        public SimpleDataBaseEngine BaseEngine { get; set; }
        public Queue<ParseTreeNode> SqlCommands { get; set; }

        

        public void GetSqlSequence(string sqlSequence)
        {
            var node = SqlParser.BuildLexicalTree(sqlSequence);
        }
    }

    class Program
    {
        static void Main()
        {
            var core = new MainFrame(new SqlSequenceParser(),
                                     new SimpleDataBaseEngine(),
                                     new Queue<ParseTreeNode>());

            for (int i = 0; i < 10000000; i++)
            {
                core.GetSqlSequence($"DROP TABLE table;");
            }


        }
    }
}
