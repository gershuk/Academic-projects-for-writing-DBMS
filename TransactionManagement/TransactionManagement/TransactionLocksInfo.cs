using System.Collections.Generic;
using ProtoBuf;

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

    [ProtoContract]
    public class TransactionLocksInfo : ITransactionLocksInfo
    {
        [ProtoMember(1)]
        public List<TableLock> TablesLocks { get; private set; }

        public TransactionLocksInfo () => TablesLocks = new List<TableLock>();

        public TransactionLocksInfo (List<TableLock> tablesLocks)
        {
            TablesLocks = new List<TableLock>();
            TablesLocks.AddRange(tablesLocks);
        }
    }
}