using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using YGOTzolkin.Service;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.UI
{
    class OptionSelector : WindowBase
    {
        private readonly RectTransform mainRect;
        private readonly List<TextMeshProUGUI> textButtons;

        public OptionSelector()
        {
            MainCanvas = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/OptionSelector") as GameObject).GetComponent<Canvas>();
            textButtons = new List<TextMeshProUGUI>(6);
            TryGetControl(out mainRect, "Panel");
        }

        public void SelectOptions(List<Tuple<int, int>> options)
        {
            MainGame.Instance.DuelWindow.SetCancelOrFinish(1);
            foreach (var pair in options)
            {
                int response;
                if (GameInfo.Instance.CurrentMessage == GameMessage.SelectIdleCmd)
                {
                    response = (pair.Item1 << 16) + 5;
                }
                else if (GameInfo.Instance.CurrentMessage == GameMessage.SelectBattleCmd)
                {
                    response = pair.Item1 << 16;
                }
                else
                {
                    response = pair.Item1;
                }
                AddText(DataService.GetDescription((uint)pair.Item2), (data) =>
                {
                    MainGame.Instance.SendCToSResponse(response);
                    MainGame.Instance.DuelWindow.SetCancelOrFinish(0);
                    Hide();
                });
            }
            Show();
        }

        public void SelectTp()
        {
            for (int i = 1; i >= 0; --i)
            {
                byte res = (byte)i;
                AddText(DataService.SysString(101 - (uint)i), (data) =>
                {
                    NetworkService.Instance.Send(new byte[]
                    {
                        0x02,
                        0x00,
                        (byte)CToSMessage.TpResult,
                        res,
                    });
                    Hide();
                });
            }
            Show();
        }

        private void AddText(string content, Action<BaseEventData> action)
        {
            var text = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/OptionText") as GameObject).GetComponent<TextMeshProUGUI>();
            textButtons.Add(text);
            text.rectTransform.localScale = Vector3.one;
            text.text = content;
            Utility.Tools.BindEvent(text.gameObject, EventTriggerType.PointerClick, action);
            text.rectTransform.SetParent(mainRect, false);
        }

        internal override void Hide()
        {
            base.Hide();
            foreach (var txt in textButtons)
            {
                txt.rectTransform.SetParent(null);
                UnityEngine.Object.Destroy(txt.gameObject);
            }
            textButtons.Clear();
        }
    }
}
