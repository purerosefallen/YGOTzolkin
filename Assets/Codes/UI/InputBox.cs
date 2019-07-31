using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YGOTzolkin.Service;
using YGOTzolkin.Utility;

namespace YGOTzolkin.UI
{
    class InputBox : WindowBase
    {
        public delegate void InputEvent(string inputValue);

        private readonly TMP_InputField input;
        private readonly Button btnOk;
        private readonly Button btnCancel;
        private readonly TextMeshProUGUI title;

        private Action cancelEvent;
        private InputEvent okEvent;

        public InputBox()
        {
            MainCanvas = Tools.LoadResource<Canvas>("Prefabs/InputBox");
            TryGetControl(out input, "IptContent");
            TryGetControl(out btnOk, "BtnOk");
            TryGetControl(out btnCancel, "BtnCancel");
            TryGetControl(out title, "TxtTitle");
            btnOk.SetButtonName(DataService.SysString(1211));
            btnOk.onClick.AddListener(OnOk);
            btnCancel.onClick.AddListener(OnCancel);
            btnCancel.SetButtonName(DataService.SysString(1212));
            input.onSubmit.AddListener((text) =>
            {
                okEvent?.Invoke(text);
                Hide();
            });
        }

        internal void Show(string title, string defaultStr, InputEvent onOk, Action onCancel)
        {
            okEvent = onOk;
            cancelEvent = onCancel;
            input.text = defaultStr;
            this.title.text = title;
            base.Show();
            input.ActivateInputField();
        }

        private void OnCancel()
        {
            cancelEvent?.Invoke();
            Hide();
        }

        private void OnOk()
        {
            okEvent?.Invoke(input.text);
            Hide();
        }
    }
}
