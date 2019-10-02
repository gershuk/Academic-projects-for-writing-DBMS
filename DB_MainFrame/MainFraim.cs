using System;
using System.Collections.Generic;
using IronySqlParser;
using DataBaseEngine;
using Irony.Parsing;
using System.Threading.Tasks;
using System.Threading;
using MultiThreadExecutor;

namespace DB_MainFrame
{
    class SqlPipelineNode
    {
        public SqlSequenceParser Parser { get; set; }
        public Task<ParseTreeNode> Task { get; }
        private readonly string _sqlSequence;

        public SqlPipelineNode(string sql)
        {
            _sqlSequence = sql ?? throw new ArgumentNullException(nameof(sql));
            Task = new Task<ParseTreeNode>(() => Parser.BuildLexicalTree(_sqlSequence));
        }
    }
    sealed class MainFrame
    {
        private Queue<SqlSequenceParser> _sqlParsers;
        private EngineCommander _engineCommander;
        private Queue<SqlPipelineNode> _sleepSqlCommands;
        private Queue<SqlPipelineNode> _workingSqlCommands;
        private object _sleepSequencesQueueLocker;
        private object _workingSequencesQueueLocker;

        private Semaphore _parsersSemaphore;
        private object _parsersLocker;
        private Thread _inputQueueControler;
        private Thread _executeControler;

        public MainFrame(int parsersCount, SimpleDataBaseEngine baseEngine)
        {
            _workingSequencesQueueLocker = new object();
            _sleepSequencesQueueLocker = new object();
            _parsersLocker = new object();

            _engineCommander = new EngineCommander(baseEngine);
            _sqlParsers = new Queue<SqlSequenceParser>();
            _sleepSqlCommands = new Queue<SqlPipelineNode>();
            _workingSqlCommands = new Queue<SqlPipelineNode>();

            _parsersSemaphore = new Semaphore(parsersCount, parsersCount, "Parsers Semaphore");
            for (var i = 0; i < parsersCount; i++)
            {
                _sqlParsers.Enqueue(new SqlSequenceParser());
            }

            _inputQueueControler = new Thread(TryParseSql);
            _inputQueueControler.Start();

            _executeControler = new Thread(TryExecuteCommand);
            _executeControler.Start();
        }


        public void GetSqlSequence(string sqlSequence)
        {
            lock (_sleepSequencesQueueLocker)
            {
                _sleepSqlCommands.Enqueue(new SqlPipelineNode(sqlSequence));
            }
        }

        private void TryParseSql()
        {
            while (true)
            {
                var queueNotEmpty = false;

                lock (_sleepSequencesQueueLocker)
                {
                    queueNotEmpty = _sleepSqlCommands?.Count > 0;
                }

                while (queueNotEmpty)
                {
                    _parsersSemaphore.WaitOne();

                    SqlPipelineNode sql;

                    lock (_sleepSequencesQueueLocker)
                    {
                        sql = _sleepSqlCommands.Dequeue();
                    }

                    lock (_parsersLocker)
                    {
                        sql.Parser = _sqlParsers.Dequeue();
                    }

                    sql.Task.Start();

                    lock (_workingSequencesQueueLocker)
                    {
                        _workingSqlCommands.Enqueue(sql);
                    }

                    lock (_sleepSequencesQueueLocker)
                    {
                        queueNotEmpty = _sleepSqlCommands?.Count > 0;
                    }
                }
            }
        }
        private void TryExecuteCommand()
        {
            while (true)
            {
                var queueNotEmpty = false;

                lock (_workingSequencesQueueLocker)
                {
                    queueNotEmpty = _workingSqlCommands?.Count > 0;
                }

                while (queueNotEmpty)
                {
                    SqlPipelineNode command;

                    lock (_workingSequencesQueueLocker)
                    {
                        command = _workingSqlCommands.Dequeue();
                    }

                    var treeNode = command.Task.Result;

                    lock (_parsersLocker)
                    {
                        _sqlParsers.Enqueue(command.Parser);
                        _parsersSemaphore.Release();
                    }

                    //ToDO:ExecuteCommandFunction
                    treeNode = treeNode.ChildNodes[0];

                    switch (treeNode.Term.Name)
                    {
                        case "DropTableStmt":
                            _engineCommander.DropTable(treeNode);
                            break;
                        case "CreateTableStmt":
                            _engineCommander.CreateTable(treeNode);
                            break;
                        case "ShowTableStmt":
                            _engineCommander.ShowTable(treeNode);
                            break;
                        default:
                            break;
                    }

                    lock (_workingSequencesQueueLocker)
                    {
                        queueNotEmpty = _workingSqlCommands?.Count > 0;
                    }
                }
            }
        }
    }

    class Program
    {

        static void Main()
        {
            var t = new SqlSequenceParser();
            t.ShowLexicalTree(t.BuildLexicalTree("DROP TABLE table.faf.aadad;"), 0);
        }
    }
}
