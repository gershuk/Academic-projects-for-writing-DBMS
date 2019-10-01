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
    sealed class MainFrame
    {
        private Queue<SqlSequenceParser> _sqlParsers;
        private SimpleDataBaseEngine _baseEngine;
        private Queue<string> _sqlSequences;
        private object _sequencesQueueLocker;
        private MultiThreadQueue<string, ParseTreeNode, SqlSequenceParser> _sqlCommandsQueue;
        private Semaphore _parsersSemaphore;
        private object _parserQueueLocker;
        private Thread _inputQueueControler;
        private ManualResetEvent _sqlRequestsNotify;
        private ManualResetEvent _executeCommandNotify;
        private readonly int _maxParsedSqlInQueue;
        private Semaphore _ParsedSqlInQueueSemaphore;
        private Thread _ExecuteCommandControler;

        public MainFrame(int parsersCount, int maxParsedSqlInQueueu, SimpleDataBaseEngine baseEngine)
        {
            _maxParsedSqlInQueue = maxParsedSqlInQueueu;

            _sqlParsers = new Queue<SqlSequenceParser>();
            for (var i = 0; i < parsersCount; i++)
            {
                _sqlParsers.Enqueue(new SqlSequenceParser());
            }
            _parserQueueLocker = new object();

            _parsersSemaphore = new Semaphore(parsersCount, parsersCount, "Sql Parsers Semaphore");
            _ParsedSqlInQueueSemaphore = new Semaphore(_maxParsedSqlInQueue, _maxParsedSqlInQueue, "Parsed Sql In Queue Semaphore");

            _baseEngine = baseEngine ?? throw new ArgumentNullException(nameof(baseEngine));

            _sqlSequences = new Queue<string>();
            _sequencesQueueLocker = new object();

            _sqlRequestsNotify = new ManualResetEvent(false);
            _inputQueueControler = new Thread(TryParseSql);
            _inputQueueControler.Start();

            _executeCommandNotify = new ManualResetEvent(false);
            _ExecuteCommandControler = new Thread(ExecuteCommand);
            _ExecuteCommandControler.Start();

            _sqlCommandsQueue = new MultiThreadQueue<string, ParseTreeNode, SqlSequenceParser>();

            _sqlCommandsQueue.CreatingDataCompleted += ParsingCompleted;
            _sqlCommandsQueue.CreatingNodeCompleted += ContinueParsing;
            _sqlCommandsQueue.CreatingNodeCompleted += ContinueExecuteCommand;
        }


        public void GetSqlSequence(string sqlSequence)
        {
            lock (_sequencesQueueLocker)
            {
                _sqlSequences.Enqueue(sqlSequence);
            }

            _sqlRequestsNotify.Set();
        }

        private void TryParseSql()
        {
            SqlSequenceParser parser;

            while (true)
            {
                //_sqlRequestsNotify.WaitOne();
                while (_sqlSequences?.Count > 0)
                {
                    _parsersSemaphore.WaitOne();

                    lock (_parserQueueLocker)
                    {
                        parser = _sqlParsers.Dequeue();
                    }

                    lock (_sequencesQueueLocker)
                    {   
                        _ParsedSqlInQueueSemaphore.WaitOne();
                        Task.Run(() => _sqlCommandsQueue.AddObjectToEnd(_sqlSequences?.Dequeue(), parser.BuildLexicalTree, parser));
                    }
                }
                //_sqlRequestsNotify.Reset();
            }
        }

        private void ExecuteCommand()
        {
            while (true)
            {
                //_executeCommandNotify.WaitOne();
                while (_sqlCommandsQueue?.GetObjectsCount() > 0)
                {
                    TakeParseTreeNode();
                }
                //_executeCommandNotify.Reset();
            }
        }

        public void ParsingCompleted(SqlSequenceParser parser)
        {
            lock (_parserQueueLocker)
            {
                if (parser != null)
                {
                    _sqlParsers.Enqueue(parser);
                }
                _parsersSemaphore.Release();
            }
        }

        public void ContinueParsing()
        {
            _sqlRequestsNotify.Set();
        }

        public void ContinueExecuteCommand()
        {
            _executeCommandNotify.Set();
        }

        public ProcessedObject<ParseTreeNode> TakeParseTreeNode()
        {
            _ParsedSqlInQueueSemaphore.Release();
            return _sqlCommandsQueue.GetFirst();
        }
    }

    class Program
    {
        static void Main()
        {
            var core = new MainFrame(10, 10, new SimpleDataBaseEngine());

            //var parser = new SqlSequenceParser();
            for (var i = 0; i < 1000000; i++)
            {
                Task.Run(() => core.GetSqlSequence($"CREATE TABLE Customers{i} (Id INT, Age FLOAT, Name VARCHAR);"));
                //parser.BuildLexicalTree($"CREATE TABLE Customers{i} (Id INT, Age FLOAT, Name VARCHAR);");
            }

            while (true) { }
        }
    }
}
