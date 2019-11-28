using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

using DataBaseEngine;

using DataBaseTable;

using DBMS_Operation;

using IronySqlParser;
using IronySqlParser.AstNodes;

using TransactionManagement;

namespace SunflowerDB
{
    public class TransactionInfo
    {
        public Guid Guid { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<OperationResult<Table>> OperationsResults { get; set; }
        public TransactionLocksInfo LocksInfo { get; set; }

        public TransactionInfo(Guid guid,
                               DateTime startTime,
                               DateTime endTime,
                               List<OperationResult<Table>> operationResults,
                               TransactionLocksInfo transactionLocksInfo)
        {
            Guid = guid;
            StartTime = startTime;
            EndTime = endTime;
            OperationsResults = operationResults ?? throw new ArgumentNullException(nameof(operationResults));
            LocksInfo = transactionLocksInfo ?? throw new ArgumentNullException(nameof(transactionLocksInfo));
        }

        public TransactionInfo(TransactionLocksInfo transactionLocksInfo) => LocksInfo = transactionLocksInfo
            ?? throw new ArgumentNullException(nameof(transactionLocksInfo));
    }

    public class SqlSequenceResult
    {
        public List<TransactionInfo> Answer { get; private set; }
    }

    public interface IDataBase
    {
        public SqlSequenceResult ExecuteSqlSequence(string sqlSequence);
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

        public SqlSequenceResult ExecuteSqlSequence(string sqlSequence)
        {
            _parsersSemaphore.WaitOne();
            _sqlParsers.TryDequeue(out var parser);
            var parseTree = parser.BuildTree(sqlSequence);
            _sqlParsers.Enqueue(parser);
            _parsersSemaphore.Release();

            var transactionListNode = (TransactionListNode)parseTree.Root.AstNode;
            var result = new SqlSequenceResult();

            foreach (var transaction in transactionListNode.TransactionNodes)
            {
                var tableLocks = new List<TableLock>();

                foreach (var command in transaction.SqlCommands)
                {
                    tableLocks.AddRange(command.GetCommandInfo());
                }

                var currentTransaction = new TransactionInfo(new TransactionLocksInfo(tableLocks));
                currentTransaction.Guid = _transactionScheduler.RegisterTransaction(currentTransaction.LocksInfo);

                _transactionScheduler.WaitTransactionResourceLock(currentTransaction.Guid);

                currentTransaction.StartTime = DateTime.Now;
                currentTransaction.OperationsResults = _engineCommander.ExecuteCommandList(transaction.SqlCommands);
                currentTransaction.EndTime = DateTime.Now;

                _transactionScheduler.RemoveTransactionResourcesLocks(currentTransaction.Guid);

                result.Answer.Add(currentTransaction);
            }

            return result;
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
