using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.Elements
{
    class CardScrollItem : MonoBehaviour
    {
        public TextMeshProUGUI TxtSummary;
        public RectTransform MainRect;
        public RawImage ImgFace;
        public RawImage ImgIcon;

        private uint code;
        private CardData data;
        private BuildCard shadow;

        internal void UpdateData(CardData data)
        {
            if (data == null)
            {
                TxtSummary.text = string.Empty;
                ImgFace.raycastTarget = false;
                ImgFace.texture = Service.TextureService.ForbiddenIcons[3];
                ImgIcon.texture = Service.TextureService.ForbiddenIcons[3];
                code = 0;
                this.data = null;
            }
            else if (code != data.Code)
            {
                code = data.Code;
                this.data = data;
                TxtSummary.text = data.Summary;
                ImgFace.raycastTarget = true;
                ImgIcon.texture = Service.TextureService.ForbiddenIcons[MainGame.Instance.DeckBuilder.CurrentFList.Query(data)];
                ImgFace.texture = Service.TextureService.GetCardTexture(code);
            }
        }

        public void OnScroll(BaseEventData data)
        {
            MainGame.Instance.Descriptor.OnTextScroll(data);
        }

        public void OnPointerEnter()
        {
            MainGame.Instance.Descriptor.Refresh(code);
        }

        public void OnPointerClicked(BaseEventData data)
        {
            if (MainGame.Instance.DeckBuilder == null || MainGame.Instance.DeckBuilder.IsShowing == false)
            {
                return;
            }
            var pointerEventData = data as PointerEventData;
            if (pointerEventData.button == PointerEventData.InputButton.Right)
            {
                MainGame.Instance.DeckBuilder.CopyCard(this.data, pointerEventData.position);
            }
        }

        public void OnBeginDrag(BaseEventData data)
        {
            if (data == null)
            {
                return;
            }
            PointerEventData pdata = data as PointerEventData;
            shadow = MainGame.Instance.DeckBuilder.CopyCard(this.data, pdata.position, true);
            if (shadow != null)
            {
                shadow.OnBeginDrag(data);
            }
        }

        public void OnEndDrag(BaseEventData data)
        {
            if (shadow != null)
            {
                shadow.OnEndDrag(data);
                shadow = null;
            }
        }
    }
}
