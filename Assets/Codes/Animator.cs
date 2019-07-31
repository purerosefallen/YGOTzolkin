using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YGOTzolkin.Utility;

namespace YGOTzolkin
{
    class Animator
    {
        #region singleton
        private static Animator instance = null;
        static Animator()
        {
            instance = new Animator();
        }
        public static Animator Instance { get { return instance; } }
        private Animator()
        {
            animQueue = new MapList<DuelAnimation>();
        }
        #endregion
        private readonly MapList<DuelAnimation> animQueue;
        private int count;

        public bool IsBlockingStopped { get; private set; }

        public void Play(DuelAnimation anim)
        {
            animQueue.Add(anim);
        }

        public void Update()
        {
            count = 0;
            for (int i = 0; i < animQueue.Count; ++i)
            {
                if (!animQueue[i].PlayNextFrame())
                {
                    animQueue.RemoveAt(i);
                    --i;
                    continue;
                }
                if (animQueue[i].Parallelizable)
                {
                    ++count;
                }
            }
            IsBlockingStopped = count == animQueue.Count;
        }

        public static IEnumerator GroupAnimationWrapper(List<IEnumerator> enumerators)
        {
            List<Stack<IEnumerator>> stacks = new List<Stack<IEnumerator>>();
            for (int i = 0; i < enumerators.Count; ++i)
            {
                stacks.Add(new Stack<IEnumerator>());
                stacks[i].Push(enumerators[i]);
            }
            while (stacks.Count > 0)
            {
                for (int i = 0; i < stacks.Count; ++i)
                {
                    var curEmt = stacks[i].Peek();
                    if (curEmt.MoveNext())
                    {
                        while (curEmt.Current != null)
                        {
                            stacks[i].Push(curEmt.Current as IEnumerator);
                            curEmt = curEmt.Current as IEnumerator;
                            curEmt.MoveNext();
                        }
                    }
                    else
                    {
                        stacks[i].Pop();
                        if (stacks[i].Count == 0)
                        {
                            stacks.RemoveAt(i);
                            --i;
                        }
                    }
                }
                yield return null;
            }
        }

        public static IEnumerator WaitTime(float durationSecs)
        {
            var elapsed = 0f;
            while (elapsed < durationSecs)
            {
                elapsed += Time.fixedDeltaTime;
                yield return null;
            }
        }
    }
}
