using System;
using System.Collections;
using System.Collections.Generic;

namespace YGOTzolkin.Utility
{
    class MapList<T> : ICollection<T>
    {
        private readonly List<T> values;
        private readonly Dictionary<T, int> indexes;

        public int Count { get { return values.Count; } }

        public T this[int i]
        {
            get
            {
                return values[i];
            }
            set
            {
                values[i] = value;
            }
        }

        public bool IsReadOnly { get; }

        public MapList()
        {
            values = new List<T>();
            indexes = new Dictionary<T, int>();
            IsReadOnly = false;
        }

        public MapList(int capacity)
        {
            values = new List<T>(capacity);
            indexes = new Dictionary<T, int>(capacity);
            IsReadOnly = false;
        }

        public bool Remove(T item)
        {
            if (!indexes.ContainsKey(item))
            {
                return false;
            }
            int idx = indexes[item];
            T last = values[values.Count - 1];
            values[idx] = last;
            indexes[last] = idx;
            values.RemoveAt(values.Count - 1);
            indexes.Remove(item);
            return true;
        }

        public void RemoveAt(int i)
        {
            Remove(values[i]);
        }

        public void Add(T value)
        {
            if (indexes.ContainsKey(value))
            {
                return;
            }
            values.Add(value);
            indexes.Add(value, values.Count - 1);
        }

        public void Clear()
        {
            values.Clear();
            indexes.Clear();
        }

        public bool Contains(T item)
        {
            return indexes.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
