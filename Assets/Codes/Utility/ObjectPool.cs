using System.Collections.Generic;

namespace YGOTzolkin.Utility
{
    interface IPoolSupporter
    {
        void Reset();
        void Disable();
    }

    class ObjectPool<T> where T : IPoolSupporter, new()
    {
        private int capaicty;
        private Stack<T> objectStack;

        internal int Count
        {
            get
            {
                return objectStack.Count;
            }
        }

        public ObjectPool()
        {
            objectStack = new Stack<T>();
        }

        internal T New()
        {
            T res;
            if (objectStack.Count == 0)
            {
                res = new T();
            }
            else
            {
                res = objectStack.Pop();
            }
            res.Reset();
            return res;
        }

        internal void Store(T t)
        {
            t.Disable();
            objectStack.Push(t);
        }

        internal void Store(List<T> list)
        {
            list.ForEach(Store);
        }
    }
}
