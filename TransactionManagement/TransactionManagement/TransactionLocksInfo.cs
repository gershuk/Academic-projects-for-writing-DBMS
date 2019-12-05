using System.Collections.Generic;
using ZeroFormatter;

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
 
    [ZeroFormattable]
    public class TransactionLocksInfo : ITransactionLocksInfo
    {
        [Index(0)]
        public virtual List<TableLock> TablesLocks { get; private set; }

        public TransactionLocksInfo()
        {
        }

        public TransactionLocksInfo(List<TableLock> tablesLocks)
        {
            tablesLocks = new List<TableLock>();
            TablesLocks.AddRange(tablesLocks);
        }
    }
}