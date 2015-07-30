using System.Collections;
using System.Collections.Generic;

namespace Tully
{
    internal class ConcurrentDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        private readonly Dictionary<K, V> _dict = new Dictionary<K, V>();

        private readonly object _padlock = new object();

        public void Add(K key, V value)
        {
            lock (_padlock)
            {
                _dict.Add(key, value);
            }
        }

        public void Remove(K key)
        {
            lock (_padlock)
            {
                _dict.Remove(key);
            }
        }

        public void RemoveAll()
        {
            lock (_padlock)
            {
                _dict.Clear();
            }
        }

        #region IEnumerable<KeyValuePair<K,V>> Members

        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
        {
            lock (_padlock)
            {
                return _dict.GetEnumerator();
            }
        }

        public IEnumerator GetEnumerator()
        {
            lock (_padlock)
            {
                return _dict.GetEnumerator();
            }
        }

        #endregion
    }
}