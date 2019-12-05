using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using DataBaseEngine;

using DataBaseType;

using IronySqlParser;
using IronySqlParser.AstNodes;

using TransactionManagement;

namespace SunflowerDB
{
    [Serializable]
    public class TransactionInfo
    {
        public List<string> Name { get; set; }
        public Guid Guid { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<OperationResult<Table>> OperationsResults { get; set; }
        public TransactionLocksInfo LocksInfo { get; set; }

        public TransactionInfo(TransactionLocksInfo transactionLocksInfo) => LocksInfo = transactionLocksInfo
            ?? throw new ArgumentNullException(nameof(transactionLocksInfo));

        public TransactionInfo(List<string> name,
                               Guid guid,
                               DateTime startTime,
                               DateTime endTime,
                               List<OperationResult<Table>> operationsResults,
                               TransactionLocksInfo locksInfo)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Guid = guid;
            StartTime = startTime;
            EndTime = endTime;
            OperationsResults = operationsResults ?? throw new ArgumentNullException(nameof(operationsResults));
            LocksInfo = locksInfo ?? throw new ArgumentNullException(nameof(locksInfo));
        }

        public TransactionInfo()
        {
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("Name:");

            foreach (var simpleId in Name)
            {
                stringBuilder.Append(simpleId);
                stringBuilder.Append(".");
            }
            stringBuilder.Append("\n");
            stringBuilder.Append($"Guid {Guid}\n");
            stringBuilder.Append($"Start Time {StartTime}\n");
            stringBuilder.Append($"End Time {EndTime}\n");

            foreach (var opResult in OperationsResults)
            {
                stringBuilder.Append($"{opResult}\n");
            }

            return stringBuilder.ToString();
        }
    }

    [ZeroFormattable]
    public class SqlSequenceResult
    {
        public List<TransactionInfo> Answer { get; private set; }
    }

    public interface IDataBase
    {
        public OperationResult<SqlSequenceResult> ExecuteSqlSequence(string sqlSequence);
    }

    public sealed class DataBase : IDisposable, IDataBase
    {
        private readonly IEngineCommander _engineCommander;
        private readonly ITransactionScheduler _transactionScheduler;
        private readonly ConcurrentQueue<SqlSequenceParser> _sqlParsers;
        private readonly Semaphore _parsersSemaphore;
        private bool _disposed = false;

        public DataBase(int parsersCount, IDataBaseEngine engine, ITransactionScheduler transactionScheduler)
        {
            _engineCommander = new EngineCommander(engine);
            _transactionScheduler = transactionScheduler;
            _sqlParsers = new ConcurrentQueue<SqlSequenceParser>();

            _parsersSemaphore = new Semaphore(parsersCount, parsersCount, "Parsers Semaphore");

            for (var i = 0; i < parsersCount; i++)
            {
                _sqlParsers.Enqueue(new SqlSequenceParser());
            }
        }

        public OperationResult<SqlSequenceResult> ExecuteSqlSequence(string sqlSequence)
        {

            var result = new SqlSequenceResult();

            #region Sequence Parsing
            _parsersSemaphore.WaitOne();
            _sqlParsers.TryDequeue(out var parser);
            var parseTree = parser.BuildTree(sqlSequence);
            _sqlParsers.Enqueue(parser);
            _parsersSemaphore.Release();
            #endregion

            //Is parsing failed
            if (parseTree.Root == null)
            {
                var message = parseTree.ParserMessages[0];
                var error = new ParsingRequestException(message.Message, message.Location.ToString());
                return new OperationResult<SqlSequenceResult>(OperationExecutionState.parserError, null, error);
            }

            #region Sequence Executing
            var transactionListNode = (TransactionListNode)parseTree.Root.AstNode;

            foreach (var transactionNode in transactionListNode.TransactionNodes)
            {
                var tableLocks = new List<TableLock>();

                foreach (var command in transactionNode.ExecuteCommnadsForNode)
                {
                    tableLocks.AddRange(command.GetTableLocks());
                }

                var transactionLocksInfo = new TransactionLocksInfo(tableLocks);

                var currentTransaction = new TransactionInfo()
                {
                    Name = transactionNode.TransactionBeginOptNode.TransactionName,
                    Guid = _transactionScheduler.RegisterTransaction(transactionLocksInfo),
                    LocksInfo = transactionLocksInfo
                };

                _transactionScheduler.WaitTransactionResourceLock(currentTransaction.Guid);

                currentTransaction.StartTime = DateTime.Now;

                var (state, exception) = _engineCommander.ExecuteCommandList(transactionNode.ExecuteCommnadsForNode);

                if (state == OperationExecutionState.performed)
                {
                    foreach (var stmt in transactionNode.StmtListNode.StmtList)
                    {
                        var table = _engineCommander.GetTableByName(stmt.SqlCommand.ReturnedTableName);
                        currentTransaction.OperationsResults.Add(table);
                    }

                    var transactionEndNode = (transactionNode as TransactionNode).TransactionEndOptNode;

                    switch (transactionEndNode.TransactionEndType)
                    {
                        case TransactionEndType.Commit:
                            _engineCommander.CommitTransaction(currentTransaction.Guid);
                            break;
                        case TransactionEndType.Rollback:
                            _engineCommander.RollBackTransaction(currentTransaction.Guid);
                            break;
                    }
                }
                else
                {
                    _engineCommander.RollBackTransaction(currentTransaction.Guid);
                    currentTransaction.OperationsResults.Add(new OperationResult<Table>(state, null, exception));
                }

                currentTransaction.EndTime = DateTime.Now;

                _transactionScheduler.RemoveTransactionResourcesLocks(currentTransaction.Guid);

                result.Answer.Add(currentTransaction);
            }
            #endregion

            return new OperationResult<SqlSequenceResult>(OperationExecutionState.performed, result, null);
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
            }
            _disposed = true;
        }

        ~DataBase()
        {
            Dispose(false);
        }
    }
}
