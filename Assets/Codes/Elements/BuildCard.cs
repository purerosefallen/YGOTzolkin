using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using YGOTzolkin.Service;
using YGOTzolkin.Utility;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.Elements
{
    class BuildCard : IPoolSupporter
    {
        private static Quaternion Rotation = Quaternion.Euler(90.001f, 90, 90);

        internal static int CompareByPosition(BuildCard left, BuildCard right)
        {
            float ly = left.Transform.position.z;
            float ry = right.Transform.position.z;
            if (Mathf.Abs(ly - ry) > 0.5 * 8.6f)
            {
                return ly > ry ? -1 : 1;
            }
            return left.Transform.position.x < right.Transform.position.x ? -1 : 1;
        }

        internal Transform Transform { get; private set; }

        internal CardData Data { get; set; }

        private readonly GameObject faceObject;
        private readonly GameObject iconObject;
        private bool dragging;
        private readonly DuelAnimation animation;

        public BuildCard()
        {
            animation = new DuelAnimation();
            Transform = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/ClientCard") as GameObject).transform;
            Transform.rotation = Rotation;
            faceObject = Transform.GetChild(0).gameObject;
            iconObject = Transform.GetChild(4).gameObject;
            iconObject.GetComponent<Renderer>().material.mainTexture = TextureService.ForbiddenIcons[3];
            for (int i = 1; i < 4; ++i)
            {
                Transform.GetChild(i).gameObject.SetActive(false);
            }
            Tools.BindEvent(faceObject, EventTriggerType.PointerEnter, OnPointerEnter);
            Tools.BindEvent(faceObject, EventTriggerType.PointerClick, OnPointerClicked);
            Tools.BindEvent(faceObject, EventTriggerType.Scroll, MainGame.Instance.Descriptor.OnTextScroll);
            Tools.BindEvent(faceObject, EventTriggerType.BeginDrag, OnBeginDrag);
            Tools.BindEvent(faceObject, EventTriggerType.EndDrag, OnEndDrag);
        }

        public void Disable()
        {
            Transform.localScale = Vector3.zero;
            Transform.localPosition = Vector3.zero;
        }

        public void Reset()
        {
            Transform.localScale = Vector3.one;
        }

        internal void SetData(CardData data)
        {
            Data = data;
            MainGame.Instance.AsyncTasks.Enqueue(() =>
            {
                faceObject.GetComponent<Renderer>().material.mainTexture = TextureService.GetCardTexture(data.Code);
                iconObject.GetComponent<Renderer>().material.mainTexture =
                    TextureService.ForbiddenIcons[MainGame.Instance.DeckBuilder.CurrentFList.Query(Data)];
            });
        }

        internal void MoveTo(Vector3 position)
        {
            var anim = Transform.FreeSmoothDampTo(position, Rotation);
            AddAnimation(anim, 0);
        }

        internal void UpdateIcon(int qualif)
        {
            iconObject.GetComponent<Renderer>().material.mainTexture = TextureService.ForbiddenIcons[qualif];
        }

        private void AddAnimation(IEnumerator anim, float waitTime = 10000)
        {
            if (animation.IsCompleted)
            {
                animation.Insert(anim, waitTime);
                Animator.Instance.Play(animation);
            }
            else
            {
                animation.Insert(anim, waitTime);
            }
        }

        private IEnumerator DragMovement()
        {
            while (dragging)
            {
                Transform.position = MainGame.Instance.MainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 95));
                yield return null;
            }
        }

        private void OnPointerEnter(BaseEventData baseEventData)
        {
            if (Data.Code > 0)
            {
                MainGame.Instance.Descriptor.Refresh(Data);
            }
        }

        private void OnPointerClicked(BaseEventData baseEventData)
        {
            if (MainGame.Instance.DeckBuilder.IsSideChanging)
            {
                return;
            }
            PointerEventData pointerEventData = baseEventData as PointerEventData;
            if (pointerEventData.button == PointerEventData.InputButton.Right)
            {
                MainGame.Instance.DeckBuilder.RemoveCard(this);
                return;
            }
            if ((pointerEventData.clickCount == 2 && pointerEventData.button == PointerEventData.InputButton.Left) ||
                (pointerEventData.button == PointerEventData.InputButton.Middle))
            {
                MainGame.Instance.DeckBuilder.CopyCard(this);
            }
        }

        internal void OnBeginDrag(BaseEventData baseEventData)
        {
            var pdata = baseEventData as PointerEventData;
            if (pdata.button == PointerEventData.InputButton.Right || pdata.button == PointerEventData.InputButton.Middle)
            {
                return;
            }
            dragging = true;
            AddAnimation(DragMovement(), 0);
        }

        internal void OnEndDrag(BaseEventData baseEventData)
        {
            dragging = false;
            MainGame.Instance.DeckBuilder.PushCard(this);
        }

    }
}
