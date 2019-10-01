using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadExecutor
{
    public class ProcessedObject<T>
    {
        public Semaphore Locker { get; set; }
        public T Data;

        public ProcessedObject() => Locker = new Semaphore(1, 1);
    }

    public class MultiThreadQueue<InObjType, SavedObjType>
    {
        public event Action<Func<InObjType, SavedObjType>> CreatingDataCompleted;
        public event Action CreatingNodeCompleted;

        private LinkedList<ProcessedObject<SavedObjType>> _processedObjects;
        private object _queueLocker;
        private List<Semaphore> _queueSemaphores;

        public MultiThreadQueue(params Semaphore[] semaphores)
        {
            _processedObjects = new LinkedList<ProcessedObject<SavedObjType>>();
            _queueSemaphores = new List<Semaphore>();
            _queueLocker = new object();

            foreach (var semaphore in semaphores)
            {
                _queueSemaphores.Add(semaphore);
            }
        }

        public void AddObjectToEnd(InObjType obj, Func<InObjType, SavedObjType> function)
        {
            var newNode = new LinkedListNode<ProcessedObject<SavedObjType>>(new ProcessedObject<SavedObjType>());

            lock (_queueLocker)
            {
                _processedObjects.AddLast(newNode);
            }

            newNode.Value.Locker.WaitOne();

            foreach (var semaphore in _queueSemaphores)
            {
                semaphore.WaitOne();
            }

            if (function != null)
            {
                newNode.Value.Data = function(obj);
            }

            newNode.Value.Locker.Release();

            foreach (var semaphore in _queueSemaphores)
            {
                semaphore.Release();
            }

            CreatingDataCompleted(function);
            CreatingNodeCompleted();
        }

        public ProcessedObject<SavedObjType> PeepFirst()
        {
            lock (_queueLocker)
            {
                return _processedObjects.First.Value;
            }
        }

        public ProcessedObject<SavedObjType> GetFirst()
        {
            lock (_queueLocker)
            {
                var node = PeepFirst();
                _processedObjects.RemoveFirst();
                return node;
            }
        }

        public int GetObjectsCount()
        {
            lock (_queueLocker)
            {
                return _processedObjects.Count;
            }
        }
    }
}
