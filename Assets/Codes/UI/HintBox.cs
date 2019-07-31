using System.Collections;
using TMPro;
using UnityEngine;

namespace YGOTzolkin.UI
{
    class HintBox : WindowBase
    {
        private readonly TextMeshProUGUI hintText;

        public HintBox()
        {
            MainCanvas = Utility.Tools.LoadResource<Canvas>("Prefabs/HintBox");
            TryGetControl(out hintText, "HintText");
        }

        internal void ShowHint(string text, float duration)
        {
            hintText.text = text;
            Animator.Instance.Play(new DuelAnimation(HintShowUp(duration)));
        }

        private IEnumerator HintShowUp(float duration)
        {
            Show();
            yield return Animator.WaitTime(duration);
            Hide();
        }
    }
}
