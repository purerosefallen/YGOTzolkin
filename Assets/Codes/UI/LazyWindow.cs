using UnityEngine;

namespace YGOTzolkin.UI
{
    abstract class LazyWindow
    {
        protected Canvas MainCanvas { get; private protected set; }

        internal bool IsShowing { get; protected private set; }

        internal virtual void Show()
        {
            if (MainCanvas == null)
            {
                LazyInitialize();
            }
            MainCanvas.gameObject.SetActive(true);
            IsShowing = true;
        }

        internal virtual void Hide()
        {
            MainCanvas?.gameObject.SetActive(false);
            IsShowing = false;
        }

        internal virtual void SetVisible(Transform control, bool visible)
        {
            control.localScale = visible ? Vector3.one : Vector3.zero;
        }

        protected abstract void LazyInitialize();

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
    }
}
