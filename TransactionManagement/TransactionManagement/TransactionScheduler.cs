using System;
using System.Collections.Generic;

namespace TransactionManagement
{
    public interface ITransactionScheduler
    {
        public Guid RegisterTransaction (TransactionLocksInfo transactionInfo);
        public void RemoveTransactionResourcesLocks (Guid transactionGuid);
        public void WaitTransactionResourceLock (Guid transactionGuid);
    }

    public class TransactionScheduler : ITransactionScheduler
    {
        private readonly Dictionary<Guid, TransactionLocksInfo> _transactions;
        private readonly Dictionary<string, TableLockQueue> _tablesLockInfo;
        private readonly object _addingLocker;

        public TransactionScheduler ()
        {
            _transactions = new Dictionary<Guid, TransactionLocksInfo>();
            _tablesLockInfo = new Dictionary<string, TableLockQueue>();
            _addingLocker = new object();
        }

        public Guid RegisterTransaction (TransactionLocksInfo transactionInfo)
        {
            lock (_addingLocker)
            {
                foreach (var tableLock in transactionInfo.TablesLocks)
                {
                    if (!_tablesLockInfo.TryGetValue(tableLock.TableName, out var queue))
                    {
                        _tablesLockInfo.Add(tableLock.TableName, queue = new TableLockQueue());
                    }

                    queue.AddLock(tableLock);
                }
            }

            var guid = Guid.NewGuid();

            _transactions.Add(guid, transactionInfo);

            return guid;
        }

        public void WaitTransactionResourceLock (Guid transactionGuid)
        {
            if (!_transactions.TryGetValue(transactionGuid, out var transaction))
            {
                throw new Exception($"Transaction guid {transactionGuid} not found");
            }

            foreach (var tableLock in transaction.TablesLocks)
            {
                tableLock.Notify.WaitOne();
            }
        }

        public void RemoveTransactionResourcesLocks (Guid transactionGuid)
        {
            if (!_transactions.TryGetValue(transactionGuid, out var transaction))
            {
                throw new Exception($"Transaction guid {transactionGuid} not found");
            }

            foreach (var tableLock in transaction.TablesLocks)
            {
                if (!_tablesLockInfo.TryGetValue(tableLock.TableName, out var queue))
                {
                    throw new Exception($"Table {tableLock.TableName} lock not found");
                }

                queue.RemoveLock(tableLock);
            }

            _transactions.Remove(transactionGuid);
        }
    }
}