using System;
using System.Collections.Generic;
using UnityEngine;
using YGOTzolkin.Utility;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.Elements
{
    class PagePanel : MonoBehaviour
    {
        public RectTransform MainRect;

        private List<CardScrollItem> PageItems;

        private Dictionary<CardScrollItem, Action> updateLine;

        private new DuelAnimation animation;

        private int capacity;
        private int sequence;

        internal void Initialize(int itemCount, int sequence)
        {
            capacity = itemCount;
            this.sequence = sequence;
            PageItems = new List<CardScrollItem>(itemCount);
            updateLine = new Dictionary<CardScrollItem, Action>(itemCount);
            animation = new DuelAnimation();
            for (int i = 0; i < itemCount; ++i)
            {
                CardScrollItem item = Tools.LoadResource<CardScrollItem>("Prefabs/CardScrollItem");
                item.MainRect.SetParent(gameObject.transform, false);
                item.MainRect.anchoredPosition3D = new Vector3(0, -66 * i, 0);
                item.UpdateData(null);
                PageItems.Add(item);
                updateLine.Add(item, null);
            }
        }

        public void Update()
        {
            int idx = Time.frameCount % (capacity * 3);
            if ((sequence + 1) * capacity > idx)
            {
                idx %= capacity;
                if (updateLine[PageItems[idx]] != null)
                {
                    updateLine[PageItems[idx]]?.Invoke();
                    updateLine[PageItems[idx]] = null;
                }
            }
        }

        internal void UpdatePage(List<CardData> datas, int startIndex)
        {
            if (startIndex < 0)
            {
                return;
            }
            int j = 0;
            for (int i = startIndex; i < datas.Count && j < capacity; ++i, ++j)
            {
                var item = PageItems[j];
                var d = datas[i];
                updateLine[item] = () => item.UpdateData(d);
            }
            for (; j < capacity; ++j)
            {
                var item = PageItems[j];
                updateLine[item] = () => item.UpdateData(null);
            }
        }

        internal void MoveTo(Vector3 position)
        {
            var anim = MainRect.SmoothDampTo(position, 0.2f);
            if (animation.IsCompleted)
            {
                animation.Insert(anim, 0);
                Animator.Instance.Play(animation);
            }
            else
            {
                animation.Insert(anim, 0);
            }
        }
    }
}
