using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TransactionManagement
{
    public interface ITableLockQueue
    {
        public void AddLock (TableLock tableLock);
        public void RemoveLock (TableLock tableLock);
    }

    public class TableLockQueue : ITableLockQueue
    {
        private (LockType LockType, int Count) _currentLock;
        private readonly Queue<TableLock> _tableLocksQueue;
        private readonly object _changeLocker;

        public TableLockQueue ()
        {
            _currentLock = (LockType.Non, 0);
            _tableLocksQueue = new Queue<TableLock>();
            _changeLocker = new object();
        }

        public void AddLock (TableLock tableLock)
        {
            lock (_changeLocker)
            {
                if ((_currentLock.LockType == tableLock.LockType && _currentLock.LockType != LockType.Update) || _currentLock.Count == 0)
                {
                    _currentLock.Count++;
                    _currentLock.LockType = tableLock.LockType;
                    tableLock.Notify.Set();
                }
                else
                {
                    _tableLocksQueue.Enqueue(tableLock);
                }
            }
        }

        public void RemoveLock (TableLock tableLock)
        {
            lock (_changeLocker)
            {
                if (_currentLock.LockType == tableLock.LockType)
                {
                    _currentLock.Count--;

                    if (_currentLock.Count == 0)
                    {
                        if (_tableLocksQueue.Count > 0)
                        {
                            var newTableLock = _tableLocksQueue.Dequeue();
                            _currentLock.Count = 1;
                            _currentLock.LockType = newTableLock.LockType;
                            newTableLock.Notify.Set();
                        }
                        else
                        {
                            _currentLock.Count = 0;
                            _currentLock.LockType = LockType.Non;
                        }
                    }
                }
                else
                {
                    throw new Exception($"Transaction lock does not match the current lock");
                }
            }
        }
    }

    public class TableLockQueueMonopolOnly : ITableLockQueue
    {
        private bool _hasMonopolLock = false;
        private readonly ConcurrentQueue<TableLock> _tableLocksQueue;
        private readonly object _changeLocker;

        public TableLockQueueMonopolOnly ()
        {
            _hasMonopolLock = false;
            _tableLocksQueue = new ConcurrentQueue<TableLock>();
            _changeLocker = new object();
        }

        public void AddLock (TableLock tableLock)
        {
            lock (_changeLocker)
            {
                if (tableLock.LockType == LockType.Update)
                {
                    if (!_hasMonopolLock)
                    {
                        _hasMonopolLock = true;
                        tableLock.Notify.Set();
                    }

                    else
                    {
                        _tableLocksQueue.Enqueue(tableLock);
                    }
                }
                else
                {
                    tableLock.Notify.Set();
                }
            }
        }

        public void RemoveLock (TableLock tableLock)
        {
            lock (_changeLocker)
            {
                if (tableLock.LockType == LockType.Update)
                {
                    _hasMonopolLock = false;
                }

                if (_tableLocksQueue.Count > 0 && !_hasMonopolLock)
                {
                    _tableLocksQueue.TryDequeue(out var newLock);
                    newLock.Notify.Set();
                    _hasMonopolLock = newLock.LockType == LockType.Update;
                }
            }
        }
    }
}
