using System;
using System.Collections.Generic;

namespace TransactionManagement
{
    public enum LockType
    {
        Non,
        Read,
        Update,
        Write
    }

    public interface ITransactionLocksInfo
    {
        public List<TableLock> TablesLocks { get; }
    }

    [Serializable]
    public class TransactionLocksInfo : ITransactionLocksInfo
    {
        public List<TableLock> TablesLocks { get; private set; }

        public TransactionLocksInfo(List<TableLock> tablesLocks)
        {
            tablesLocks = new List<TableLock>();
            TablesLocks.AddRange(tablesLocks);
        }
    }
}