using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YGOTzolkin.UI
{
    class WindowBase
    {
        protected Canvas MainCanvas { get; private protected set; }

        internal bool IsShowing { get; protected private set; }

        internal virtual void Show()
        {
            MainCanvas.gameObject.SetActive(true);
            MainCanvas.enabled = true;
            IsShowing = true;
        }

        internal virtual void Hide()
        {
            MainCanvas.enabled = false;
            IsShowing = false;
            MainCanvas.gameObject.SetActive(false);
        }

        internal virtual void SetVisible(Transform control, bool visible)
        {
            control.localScale = visible ? Vector3.one : Vector3.zero;
        }

        protected T GetControl<T>(string name)
        {
            foreach (var t in MainCanvas.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.ToLower() == name.ToLower())
                {
                    return t.GetComponent<T>();
                }
            }
            return default;
        }

        protected bool TryGetControl<T>(out T control, string name)
        {
            foreach (var t in MainCanvas.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.ToLower() == name.ToLower())
                {
                    control = t.GetComponent<T>();
                    return true;
                }
            }
            control = default;
            return false;
        }

        protected void SetInputPlaceHolder(TMP_InputField input,string text)
        {
            input.placeholder.GetComponent<TextMeshProUGUI>().text = text;
        }
    }
}
