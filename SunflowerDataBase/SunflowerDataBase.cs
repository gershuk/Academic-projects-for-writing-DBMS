using System;
using System.Collections.Generic;
using IronySqlParser;
using DataBaseEngine;
using Irony.Parsing;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace SunflowerDataBase
{

    public class SqlCommandResult
    {
        public OperationResult<string> Answer { get; set; }
        public ManualResetEvent AnswerNotify;
        public override string ToString() => Answer.Result + " " + Answer.State;
    }

    sealed public class SunflowerDataBase : IDisposable
    {
        private class SqlCommand
        {
            public SqlSequenceParser Parser { get; set; }
            public Task<ParseTree> Task { get; }
            public SqlCommandResult CommandResult { get; set; }

            private readonly string _sqlSequence;

            public SqlCommand(string sql)
            {
                CommandResult = new SqlCommandResult
                {
                    Answer = new OperationResult<string>(OperationExecutionState.notProcessed, new string("")),
                    AnswerNotify = new ManualResetEvent(false)
                };
                _sqlSequence = sql ?? throw new ArgumentNullException(nameof(sql));
                Task = new Task<ParseTree>(() => Parser.BuildLexicalTree(_sqlSequence));
            }
        }

        private EngineCommander _engineCommander;

        private ConcurrentQueue<SqlSequenceParser> _sqlParsers;
        private ConcurrentQueue<SqlCommand> _waitingSqlCommands;
        private ConcurrentQueue<SqlCommand> _workingSqlCommands;

        private Semaphore _parsersSemaphore;

        private Thread _inputQueueControler;
        private Thread _executeControler;

        private bool _disposed = false;
        private bool _stopWorking = false;

        public SunflowerDataBase(int parsersCount, DataBaseEngineMain baseEngine)
        {
            _engineCommander = new EngineCommander(baseEngine);

            _sqlParsers = new ConcurrentQueue<SqlSequenceParser>();
            _waitingSqlCommands = new ConcurrentQueue<SqlCommand>();
            _workingSqlCommands = new ConcurrentQueue<SqlCommand>();

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


        public SqlCommandResult SendSqlSequence(string sqlSequence)
        {
            var command = new SqlCommand(sqlSequence);
            _waitingSqlCommands.Enqueue(command);
            return command.CommandResult;
        }

        private void TryParseSql()
        {
            while (true)
            {
                if (_stopWorking)
                {
                    return;
                }


                while (_waitingSqlCommands.TryDequeue(out var sqlCommand))
                {
                    _parsersSemaphore.WaitOne();
                    _sqlParsers.TryDequeue(out var parser);
                    sqlCommand.Parser = parser;

                    sqlCommand.Task.Start();

                    _workingSqlCommands.Enqueue(sqlCommand);
                }
            }
        }
        private void TryExecuteCommand()
        {
            while (true)
            {
                if (_stopWorking)
                {
                    return;
                }

                while (_workingSqlCommands.TryDequeue(out var sqlCommand))
                {

                    var parserTree = sqlCommand.Task.Result;

                    _sqlParsers.Enqueue(sqlCommand.Parser);
                    sqlCommand.Parser = null;
                    _parsersSemaphore.Release();


                    if (parserTree.Root == null)
                    {
                        var answer = sqlCommand.CommandResult.Answer;
                        answer.State = OperationExecutionState.parserError;
                        answer.Result = parserTree.ParserMessages[0].Message + " " + parserTree.ParserMessages[0].Location.ToString();
                    }
                    else
                    {
                        var treeNode = parserTree.Root.ChildNodes[0];
                        sqlCommand.CommandResult.Answer = _engineCommander.ExecuteCommand(treeNode);
                        //_engineCommander.Engine.Commit();
                    }

                    sqlCommand.CommandResult.AnswerNotify.Set();
                }
            }
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _parsersSemaphore.Dispose();
                _stopWorking = true;
                _inputQueueControler.Join();
                _executeControler.Join();
            }
            _disposed = true;
        }

        ~SunflowerDataBase()
        {
            Dispose(false);
        }
    }
}
