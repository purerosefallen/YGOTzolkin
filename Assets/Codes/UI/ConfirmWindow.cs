using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YGOTzolkin.Utility;
using static YGOTzolkin.Service.DataService;

namespace YGOTzolkin.UI
{
    class ConfirmWindow : WindowBase
    {
        private readonly Button btnOk;
        private readonly Button btnCancel;
        private readonly TextMeshProUGUI tmpTitle;
        private readonly RectTransform screenMask;

        public ConfirmWindow()
        {
            MainCanvas = Tools.LoadResource<Canvas>("Prefabs/ConfirmWindow");
            TryGetControl(out btnOk, "BtnOK");
            TryGetControl(out btnCancel, "BtnCancel");
            TryGetControl(out tmpTitle, "TMPTitle");
            TryGetControl(out screenMask, "ScreenMask");
            screenMask.localScale = Vector3.zero;
            btnOk.SetButtonName(SysString(1211));
            btnCancel.SetButtonName(SysString(1212));
        }

        internal void SelectYesNo(string title)
        {
            screenMask.localScale = Vector3.zero;
            btnCancel.onClick.RemoveAllListeners();
            btnOk.onClick.RemoveAllListeners();
            btnOk.onClick.AddListener(() =>
            {
                MainGame.Instance.SendCToSResponse(1);
                Hide();
            });
            btnCancel.onClick.AddListener(Cancel);
            tmpTitle.text = title;
            Show();
        }

        internal void SelectYesNo(string title, Action onOk, Action onCancel)
        {
            screenMask.localScale = Vector3.one;
            btnCancel.onClick.RemoveAllListeners();
            btnOk.onClick.RemoveAllListeners();
            btnOk.onClick.AddListener(() =>
            {
                onOk?.Invoke();
                Hide();
            });
            btnCancel.onClick.AddListener(() =>
            {
                onCancel?.Invoke();
                Hide();
            });
            tmpTitle.text = title;
            Show();
        }

        internal void Cancel()
        {
            MainGame.Instance.SendCToSResponse(0);
            Hide();
        }
    }
}
