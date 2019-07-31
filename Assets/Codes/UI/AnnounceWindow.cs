using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YGOTzolkin.Service;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.UI
{
    class AnnounceWindow : WindowBase
    {
        private readonly TMPro.TMP_InputField input;
        private readonly RectTransform scrollContent;
        private readonly GraphicRaycaster raycaster;
        private readonly PointerEventData pdata;
        private readonly List<RaycastResult> raycastResults;

        private readonly List<Toggle> toggles;
        private Dictionary<Transform, CardData> toggleCodes;
        private List<int> opCodes;

        public AnnounceWindow()
        {
            MainCanvas = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/AnnounceWindow") as GameObject).GetComponent<Canvas>();
            raycaster = MainCanvas.GetComponent<GraphicRaycaster>();
            TryGetControl(out scrollContent, "Content");
            TryGetControl(out input, "TMPName");
            toggles = new List<Toggle>(50);
            toggleCodes = new Dictionary<Transform, CardData>(50);
            input.onSubmit.AddListener(OnSubmit);
            pdata = new PointerEventData(MainGame.Instance.EventSystem);
            raycastResults = new List<RaycastResult>();
        }

        internal override void Hide()
        {
            MainGame.Instance.FrameActions.Remove(Update);
            base.Hide();
            toggles.ForEach(DestoryToggle);
            toggles.Clear();
            toggleCodes.Clear();
        }

        internal override void Show()
        {
            MainGame.Instance.FrameActions.Add(Update);
            base.Show();
            input.ActivateInputField();
        }

        internal void AnnounceName(List<int> opCodes)
        {
            this.opCodes = opCodes;
            Show();
        }

        private void Update()
        {
            pdata.position = Input.mousePosition;
            raycastResults.Clear();
            raycaster.Raycast(pdata, raycastResults);
            foreach (var trans in raycastResults)
            {
                if (trans.gameObject.transform.CompareTag("OptionImage"))
                {
                    if (toggleCodes.TryGetValue(trans.gameObject.transform, out CardData data))
                    {
                        MainGame.Instance.Descriptor.Refresh(data);
                        break;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                input.ActivateInputField();
            }
        }

        private void SetToggle(Toggle toggle, uint code)
        {
            var img = toggle.transform.GetChild(0).GetComponent<Image>();
            var texture = TextureService.GetCardTexture(code);
            img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            toggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    MainGame.Instance.SendCToSResponse((int)code);
                    Hide();
                }
            });
        }

        private void DestoryToggle(Toggle toggle)
        {
            toggle.transform.SetParent(null);
            UnityEngine.Object.Destroy(toggle.gameObject);
        }

        private void OnSubmit(string content)
        {
            List<CardData> cards;
            cards = DataService.Search(content, opCodes);
            int upbound = cards.Count > 50 ? 50 : cards.Count;
            toggleCodes.Clear();
            if (toggles.Count > 0)
            {
                int count = toggles.Count > cards.Count ? cards.Count : toggles.Count;
                for (int i = 0; i < count; ++i)
                {
                    toggles[i].onValueChanged.RemoveAllListeners();
                    SetToggle(toggles[i], cards[i].Code);
                    toggleCodes.Add(toggles[i].transform.GetChild(0).transform, cards[i]);
                }
                if (toggles.Count > cards.Count)
                {
                    for (int i = cards.Count; i < toggles.Count; ++i)
                    {
                        DestoryToggle(toggles[i]);
                    }
                    toggles.RemoveRange(cards.Count, toggles.Count - cards.Count);
                }
                else if (toggles.Count < cards.Count)
                {
                    for (int i = toggles.Count; i < cards.Count; ++i)
                    {
                        var toggle = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/OptionImage") as GameObject).GetComponent<Toggle>();
                        toggles.Add(toggle);
                        toggleCodes.Add(toggle.transform.GetChild(0).transform, cards[i]);
                        toggle.transform.SetParent(scrollContent, false);
                        toggle.transform.localScale = Vector3.one;
                        SetToggle(toggle, cards[i].Code);
                    }
                }
            }
            else
            {
                for (int i = 0; i < upbound; ++i)
                {
                    var toggle = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/OptionImage") as GameObject).GetComponent<Toggle>();
                    toggles.Add(toggle);
                    toggle.transform.SetParent(scrollContent, false);
                    toggle.transform.localScale = Vector3.one;
                    toggleCodes.Add(toggle.transform.GetChild(0).transform, cards[i]);
                    SetToggle(toggle, cards[i].Code);
                }
            }
        }
    }
}
