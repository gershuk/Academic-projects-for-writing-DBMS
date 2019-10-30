using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using DataBaseEngine;

using DataBaseErrors;

using DataBaseTable;

using Irony.Parsing;

using IronySqlParser;

namespace SunflowerDB
{
    public class SqlCommandResult
    {
        public OperationResult<Table> Answer { get; set; }
        public ManualResetEvent AnswerNotify;
        public override string ToString() => Answer.State switch
        {
            OperationExecutionState.parserError => $"{Answer.OperationException}",
            OperationExecutionState.failed => $"{Answer.OperationException}",
            OperationExecutionState.performed => $"{Answer.Result}",
            OperationExecutionState.notProcessed => $"Not processed"
        };

    }

    sealed public class DataBase : IDisposable
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
                    Answer = new OperationResult<Table>(OperationExecutionState.notProcessed, null),
                    AnswerNotify = new ManualResetEvent(false)
                };

                _sqlSequence = sql ?? throw new ArgumentNullException(nameof(sql));

                Task = new Task<ParseTree>(() => Parser.BuildLexicalTree(_sqlSequence));
            }
        }

        private readonly EngineCommander _engineCommander;

        private readonly ConcurrentQueue<SqlSequenceParser> _sqlParsers;
        private readonly ConcurrentQueue<SqlCommand> _waitingSqlCommands;
        private readonly ConcurrentQueue<SqlCommand> _workingSqlCommands;

        private readonly Semaphore _parsersSemaphore;

        private readonly Thread _inputQueueControler;
        private readonly Thread _executeControler;

        private bool _disposed = false;
        private bool _stopWorking = false;

        public DataBase(int parsersCount, DataBaseEngineMain baseEngine)
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
            if (!_stopWorking)
            {
                var command = new SqlCommand(sqlSequence);
                _waitingSqlCommands.Enqueue(command);
                return command.CommandResult;
            }
            else
            {
                return null;
            }

        }

        private void TryParseSql()
        {
            while (!_stopWorking)
            {
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
            while (!_stopWorking)
            {
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
                        answer.Result = null;

                        var parserMessage = parserTree.ParserMessages[0];

                        answer.OperationException = new ParsingRequestError(parserMessage.Message, parserMessage.Location.ToString());
                    }
                    else
                    {
                        var treeNode = parserTree.Root.ChildNodes[0];
                        sqlCommand.CommandResult.Answer = _engineCommander.ExecuteCommand(treeNode);
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

        ~DataBase()
        {
            Dispose(false);
        }
    }
}
