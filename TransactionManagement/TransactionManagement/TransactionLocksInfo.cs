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

    internal interface ITransactionLocksInfo
    {
        public List<TableLock> TablesLocks { get; }
    }

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