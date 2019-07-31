using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YGOTzolkin.Service;
using YGOTzolkin.Utility;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.UI
{
    class CardDescriptor : WindowBase
    {
        private readonly RawImage imgCard;
        private readonly TMP_InputField iptCode;
        private readonly TextMeshProUGUI tmpDescription;
        private readonly ScrollRect scrollRect;
        private readonly TextMeshProUGUI floatingPanel;
        private readonly LinkedList<CardData> cachedDatas;
        private LinkedListNode<CardData> currentNode;
        private readonly int capacity;
        private float dragPoint;

        public CardDescriptor()
        {
            MainCanvas = Tools.LoadResource<Canvas>("Prefabs/Descriptor");
            imgCard = GetControl<RawImage>("ImgCard");
            Tools.BindEvent(imgCard.gameObject, EventTriggerType.Scroll, OnImageScroll);
            Tools.BindEvent(imgCard.gameObject, EventTriggerType.EndDrag, OnEndDrag);
            iptCode = GetControl<TMP_InputField>("IptCode");
            tmpDescription = GetControl<TextMeshProUGUI>("TMPDescription");
            scrollRect = GetControl<ScrollRect>("ScrollView");
            capacity = 100;
            cachedDatas = new LinkedList<CardData>();
            //temp
            floatingPanel = Tools.LoadResource<TextMeshProUGUI>("Prefabs/FloatingPanel");
            floatingPanel.rectTransform.SetParent(MainCanvas.transform);
            floatingPanel.rectTransform.localScale = Vector3.zero;
        }

        internal override void Show()
        {
            if(IsShowing)
            {
                return;
            }
            MainGame.Instance.FrameActions.Add(Update);
            base.Show();
        }

        internal override void Hide()
        {
            base.Hide();
            MainGame.Instance.FrameActions.Remove(Update);
        }

        internal void Refresh(CardData data)
        {
            if (data != null)
            {
                if(currentNode!=null)
                {
                    cachedDatas.AddAfter(currentNode, data);
                    currentNode = currentNode.Next;
                }
                else
                {
                    cachedDatas.AddLast(data);
                    currentNode = cachedDatas.Last;
                }
                if (cachedDatas.Count >= capacity)
                {
                    cachedDatas.RemoveFirst();
                }
                RefreshCard(data);
            }
        }

        internal void Refresh(uint code)
        {
            if (code > 0)
            {
                Refresh(DataService.GetCardData(code));
            }
        }

        internal void ShowPanel(string text)
        {
            var mpos = Input.mousePosition;
            floatingPanel.text = text;
            floatingPanel.rectTransform.localScale = Vector3.one;
            var rect = floatingPanel.rectTransform.rect;
            if (mpos.y + rect.height > Screen.height)
            {
                mpos.x += rect.width / 2;
                mpos.y -= rect.height / 2;
            }
            else
            {
                mpos.y += rect.height / 2;
                mpos.x += rect.width / 2;
            }
            floatingPanel.rectTransform.position = mpos;
        }

        internal void HidePanel()
        {
            floatingPanel.rectTransform.localScale = Vector3.zero;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                var orgpos = scrollRect.content.localPosition;
                orgpos.y -= 50;
                scrollRect.content.localPosition = orgpos;
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                var orgpos = scrollRect.content.localPosition;
                orgpos.y += 50;
                scrollRect.content.localPosition = orgpos;
            }
        }

        private void RefreshCard(CardData data)
        {
            iptCode.text = data.Code.ToString();
            tmpDescription.text = data.TotalInfo;
            imgCard.texture = TextureService.GetCardTexture(data.Code);
        }

        private void TurnPage(bool back = true)
        {
            if (back)
            {
                if (currentNode != null && currentNode.Previous != null)
                {
                    var cd = currentNode.Previous.Value;
                    currentNode = currentNode.Previous;
                    RefreshCard(cd);
                }
            }
            else
            {
                if (currentNode != null && currentNode.Next != null)
                {
                    var cd = currentNode.Next.Value;
                    currentNode = currentNode.Next;
                    RefreshCard(cd);
                }
            }
        }

        #region event
        internal void OnTextScroll(BaseEventData data)
        {
            scrollRect.OnScroll(data as PointerEventData);
        }

        private void OnImageScroll(BaseEventData data)
        {
            var pdata = data as PointerEventData;
            TurnPage(pdata.scrollDelta.y > 0);
        }

        private void OnEndDrag(BaseEventData data)
        {
            var pdata = data as PointerEventData;
            TurnPage(pdata.delta.x > 0);
        }
        #endregion
    }
}
