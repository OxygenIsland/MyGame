using System.Collections.Generic;

namespace StarWorld.Common.Utility
{
    public class FixedSizeQueue<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly int _maxSize;

        public FixedSizeQueue(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void Enqueue(T item)
        {
            if (_queue.Count >= _maxSize)
            {
                _queue.Dequeue();
            }
            _queue.Enqueue(item);
        }

        public T Dequeue()
        {
            return _queue.Dequeue();
        }

        public int Count => _queue.Count;

        public T Peek()
        {
            return _queue.Peek();
        }

        public void Clear()
        {
            _queue.Clear();
        }

        public bool Contains(T item)
        {
            return _queue.Contains(item);
        }
    }
}