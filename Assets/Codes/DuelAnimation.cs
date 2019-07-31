using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YGOTzolkin
{
    class DuelAnimation
    {
        private readonly Stack<IEnumerator> exeStack;
        private float serialDuration;
        private float elapsed;

        public bool IsCompleted
        {
            get
            {
                return exeStack.Count == 0;
            }
        }

        public bool Parallelizable { get; private set; }

        public DuelAnimation()
        {
            exeStack = new Stack<IEnumerator>(10);
            serialDuration = 100000;
            elapsed = 0;
            Parallelizable = false;
        }

        public DuelAnimation(IEnumerator enumerator, float serialDuration = 100000) : this()
        {
            exeStack.Push(enumerator);
            this.serialDuration = serialDuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>whether it can still play</returns>
        public bool PlayNextFrame()
        {
            var curAnim = exeStack.Peek();
            if (curAnim.MoveNext())
            {
                while (curAnim.Current != null)
                {
                    exeStack.Push(curAnim.Current as IEnumerator);
                    curAnim = curAnim.Current as IEnumerator;
                    curAnim.MoveNext();
                }
                Parallelizable = (elapsed += Time.fixedDeltaTime) > serialDuration;
                return true;
            }
            else
            {
                exeStack.Pop();
                return exeStack.Count != 0;
            }
        }

        public void Insert(IEnumerator anim, float serial = 10000)
        {
            exeStack.Clear();
            exeStack.Push(anim);
            serialDuration = serial;
            elapsed = 0;
            Parallelizable = false;
        }
    }
}
