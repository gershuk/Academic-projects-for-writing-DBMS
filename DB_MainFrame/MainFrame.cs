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
    class SqlCommand
    {
        public SqlSequenceParser Parser { get; set; }
        public Task<ParseTree> Task { get; }
        public OperationResult<string> Answer { get; set; }
        public ManualResetEvent AnswerNotify;

        private readonly string _sqlSequence;

        public SqlCommand(string sql)
        {
            Answer = new OperationResult<string>(OperationExecutionState.notProcessed, new string(""));
            AnswerNotify = new ManualResetEvent(false);
            _sqlSequence = sql ?? throw new ArgumentNullException(nameof(sql));
            Task = new Task<ParseTree>(() => Parser.BuildLexicalTree(_sqlSequence));
        }

        public override string ToString() => Answer.Result + " " + Answer.State;
    }
    sealed class MainFrame : IDisposable
    {
        private EngineCommander _engineCommander;

        private Queue<SqlSequenceParser> _sqlParsers;
        private Queue<SqlCommand> _sleepSqlCommands;
        private Queue<SqlCommand> _workingSqlCommands;

        private Semaphore _parsersSemaphore;

        private object _sleepSequencesQueueLocker;
        private object _workingSequencesQueueLocker;
        private object _parsersLocker;

        private Thread _inputQueueControler;
        private Thread _executeControler;

        private bool _disposed = false;
        private bool _stopWorking = false;

        public MainFrame(int parsersCount, SimpleDataBaseEngine baseEngine)
        {
            _workingSequencesQueueLocker = new object();
            _sleepSequencesQueueLocker = new object();
            _parsersLocker = new object();

            _engineCommander = new EngineCommander(baseEngine);

            _sqlParsers = new Queue<SqlSequenceParser>();
            _sleepSqlCommands = new Queue<SqlCommand>();
            _workingSqlCommands = new Queue<SqlCommand>();

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


        public SqlCommand SendSqlSequence(string sqlSequence)
        {
            var command = new SqlCommand(sqlSequence);

            lock (_sleepSequencesQueueLocker)
            {
                _sleepSqlCommands.Enqueue(command);
            }

            return command;
        }

        private void TryParseSql()
        {
            while (true)
            {
                if (_stopWorking)
                {
                    return;
                }

                var queueNotEmpty = false;

                lock (_sleepSequencesQueueLocker)
                {
                    queueNotEmpty = _sleepSqlCommands?.Count > 0;
                }

                while (queueNotEmpty)
                {
                    _parsersSemaphore.WaitOne();

                    SqlCommand sqlCommand;

                    lock (_sleepSequencesQueueLocker)
                    {
                        sqlCommand = _sleepSqlCommands.Dequeue();
                    }

                    lock (_parsersLocker)
                    {
                        sqlCommand.Parser = _sqlParsers.Dequeue();
                    }

                    sqlCommand.Task.Start();

                    lock (_workingSequencesQueueLocker)
                    {
                        _workingSqlCommands.Enqueue(sqlCommand);
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
                if (_stopWorking)
                {
                    return;
                }

                var queueNotEmpty = false;

                lock (_workingSequencesQueueLocker)
                {
                    queueNotEmpty = _workingSqlCommands?.Count > 0;
                }

                while (queueNotEmpty)
                {
                    SqlCommand command;

                    lock (_workingSequencesQueueLocker)
                    {
                        command = _workingSqlCommands.Dequeue();
                    }

                    var parserTree = command.Task.Result;

                    lock (_parsersLocker)
                    {
                        _sqlParsers.Enqueue(command.Parser);
                        command.Parser = null;
                        _parsersSemaphore.Release();
                    }

                    if (parserTree.Root == null)
                    {
                        var answer = command.Answer;
                        answer.State = OperationExecutionState.parserError;
                        answer.Result = parserTree.ParserMessages[0].Message + " " + parserTree.ParserMessages[0].Location.ToString();
                    }
                    else
                    {
                        var treeNode = parserTree.Root.ChildNodes[0];

                        command.Answer = treeNode.Term.Name switch
                        {
                            "DropTableStmt" => _engineCommander.DropTable(treeNode),
                            "CreateTableStmt" => _engineCommander.CreateTable(treeNode),
                            "ShowTableStmt" => _engineCommander.ShowTable(treeNode)
                        };
                    }

                    command.AnswerNotify.Set();

                    lock (_workingSequencesQueueLocker)
                    {
                        queueNotEmpty = _workingSqlCommands?.Count > 0;
                    }
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

        ~MainFrame()
        {
            Dispose(false);
        }
    }

    class Program
    {

        static void Main()
        {
            var core = new MainFrame(10, new SimpleDataBaseEngine());
            var exitState = true;
            Console.WriteLine("Hello!");
            Console.WriteLine("Please enter your sql request.");
            Console.WriteLine("If you want to quit write 'exit'."); 
            while (exitState)
            {
                var input = Console.ReadLine();
                if (input == "exit")
                {
                    exitState = false;
                    core.Dispose();
                }
                else
                {
                    var ans = core.SendSqlSequence(input);
                    ans.AnswerNotify.WaitOne();
                    Console.WriteLine(ans);
                    Console.WriteLine("--------------------------------------------------------------------------------");
                }
                //"CREATE TABLE Customers (Id INT,Age FLOAT, Name VARCHAR(20));"
            }
        }
    }
}
