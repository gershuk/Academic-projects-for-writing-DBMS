using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

using DataBaseEngine;

using DataBaseType;

using IronySqlParser;
using IronySqlParser.AstNodes;

using TransactionManagement;
namespace SunflowerDB
{
    public interface IDataBase
    {
        public OperationResult<SqlSequenceResult> ExecuteSqlSequence (string sqlSequence);
    }

    public sealed class DataBase : IDisposable, IDataBase
    {
        private readonly IEngineCommander _engineCommander;
        private readonly ITransactionScheduler _transactionScheduler;
        private readonly ConcurrentQueue<SqlSequenceParser> _sqlParsers;
        private readonly Semaphore _parsersSemaphore;
        private bool _disposed = false;

        public DataBase (int parsersCount, IDataBaseEngine engine, ITransactionScheduler transactionScheduler)
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

        public OperationResult<SqlSequenceResult> ExecuteSqlSequence (string sqlSequence)
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
                var error = new ParsingRequestError(message.Message, message.Location.ToString());
                return new OperationResult<SqlSequenceResult>(ExecutionState.parserError, null, error);
            }

            #region Sequence Executing
            var transactionListNode = (TransactionListNode)parseTree.Root.AstNode;

            foreach (var transactionNode in transactionListNode.TransactionNodes)
            {
                var tableLocks = new List<TableLock>();

                foreach (var command in transactionNode.CommnadsForNode)
                {
                    tableLocks.AddRange(command.GetTableLocks());
                }

                var transactionLocksInfo = new TransactionLocksInfo(tableLocks);

                var transaction = new TransactionInfo()
                {
                    Name = transactionNode.TransactionBeginOptNode?.TransactionName,
                    Guid = _transactionScheduler.RegisterTransaction(transactionLocksInfo),
                    LocksInfo = transactionLocksInfo
                };

                transaction.Name ??= new List<string>() { transaction.Guid.ToString() };

                _transactionScheduler.WaitTransactionResourceLock(transaction.Guid);

                transaction.StartTime = DateTime.Now;
                _engineCommander.StartTransaction(transaction.Guid);

                var (state, exception) = _engineCommander.ExecuteCommands(transaction.Guid, transactionNode.CommnadsForNode);

                if (state == ExecutionState.performed)
                {
                    foreach (var stmt in transactionNode.StmtListNode.StmtList)
                    {
                        var table = _engineCommander.GetTableByName(transaction.Guid, stmt.SqlCommand.ReturnedTableName);
                        transaction.OperationsResults.Add(table);
                    }

                    var transactionEndNode = (transactionNode as TransactionNode).TransactionEndOptNode;

                    switch (transactionEndNode.TransactionEndType)
                    {
                        case TransactionEndType.Commit:
                            _engineCommander.CommitTransaction(transaction.Guid);
                            break;
                        case TransactionEndType.Rollback:
                            _engineCommander.RollBackTransaction(transaction.Guid);
                            break;
                    }
                }
                else
                {
                    _engineCommander.RollBackTransaction(transaction.Guid);
                    transaction.OperationsResults.Add(new OperationResult<Table>(state, null, exception));
                }

                transaction.EndTime = DateTime.Now;

                _transactionScheduler.RemoveTransactionResourcesLocks(transaction.Guid);

                result.Answer.Add(transaction);
            }
            #endregion

            return new OperationResult<SqlSequenceResult>(ExecutionState.performed, result, null);
        }

        public void Dispose ()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        private void Dispose (bool disposing)
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

        ~DataBase ()
        {
            Dispose(false);
        }
    }
}
