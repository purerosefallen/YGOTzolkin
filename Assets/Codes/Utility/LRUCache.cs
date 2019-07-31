using System.Collections.Generic;

namespace YGOTzolkin.Utility
{
    class LRUCache<TKey, TValue>
    {
        private Dictionary<TKey, LinkedListNode<LRUElement<TKey, TValue>>> cacheMap;
        private LinkedList<LRUElement<TKey, TValue>> valuesList;
        private readonly int capacity;

        public LRUCache(int capacity)
        {
            cacheMap = new Dictionary<TKey, LinkedListNode<LRUElement<TKey, TValue>>>();
            valuesList = new LinkedList<LRUElement<TKey, TValue>>();
            this.capacity = capacity;
        }

        public TValue Get(TKey key)
        {
            if (cacheMap.TryGetValue(key, out LinkedListNode<LRUElement<TKey, TValue>> knode))
            {
                var node = knode;
                valuesList.Remove(node);
                valuesList.AddFirst(node);
                return node.Value.Value;
            }
            else
            {
                return default;
            }
        }

        public void Put(TKey key, TValue value)
        {
            if (cacheMap.TryGetValue(key, out LinkedListNode<LRUElement<TKey, TValue>> knode))
            {
                var node = knode;
                valuesList.Remove(node);
                valuesList.AddFirst(node);
                node.Value.Value = value;
                return;
            }
            if (cacheMap.Count >= capacity)
            {
                var last = valuesList.Last;
                cacheMap.Remove(last.Value.Key);
                valuesList.RemoveLast();
            }
            var newnode = new LinkedListNode<LRUElement<TKey, TValue>>(new LRUElement<TKey, TValue>(key, value));
            cacheMap.Add(key, newnode);
            valuesList.AddFirst(newnode);
        }
    }
    class LRUElement<TKey, TValue>
    {
        public TKey Key { get; }
        public TValue Value { get; set; }
        public LRUElement(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}
