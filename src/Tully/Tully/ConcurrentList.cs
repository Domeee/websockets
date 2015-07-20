using System.Collections;
using System.Collections.Generic;

namespace Tully
{
    internal class ConcurrentList<T> : IEnumerable<T>
    {
        private readonly List<T> _list = new List<T>();

        private readonly object _padlock = new object();

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            lock (_padlock)
            {
                return _list.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_padlock)
            {
                return _list.GetEnumerator();
            }
        }

        public void Add(T element)
        {
            lock (_padlock)
            {
                _list.Add(element);
            }
        }
    }
}