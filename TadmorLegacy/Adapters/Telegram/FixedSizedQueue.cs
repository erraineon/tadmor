using System.Collections.Concurrent;

namespace Tadmor.Adapters.Telegram
{
    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        private readonly object _syncObject = new object();
        private readonly int _size;

        public FixedSizedQueue(int size)
        {
            _size = size;
        }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            lock (_syncObject)
            {
                while (Count > _size) TryDequeue(out _);
            }
        }
    }
}