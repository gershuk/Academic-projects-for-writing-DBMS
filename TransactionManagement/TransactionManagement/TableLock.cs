using System;
using System.Collections.Generic;
using System.Threading;

using ProtoBuf;

namespace TransactionManagement
{
    public interface ITableLock
    {
        public LockType LockType { get; }
        public ManualResetEvent Notify { get; }
        public List<string> TableName { get; }
    }

    [ProtoContract]
    public class TableLock : ITableLock
    {
        [ProtoMember(1)]
        public LockType LockType { get; private set; }

        [ProtoMember(2)]
        public List<string> TableName { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "<Ожидание>")]
        public ManualResetEvent Notify { get; private set; }

        public TableLock (LockType lockType, List<string> name, ManualResetEvent notify)
        {
            LockType = lockType;
            TableName = name ?? throw new ArgumentNullException(nameof(name));
            Notify = new ManualResetEvent(false);
        }
    }
}