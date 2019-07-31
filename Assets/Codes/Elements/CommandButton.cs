using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using YGOTzolkin.Utility;

namespace YGOTzolkin.Elements
{
    class CommandButton : IPoolSupporter
    {
        public GameObject ButtonObject;
        private int response;
        private List<Tuple<int, int>> activateOptions;

        public CommandButton()
        {
            ButtonObject = MonoBehaviour.Instantiate(Resources.Load("Prefabs/CommandButton") as GameObject);
            Tools.BindEvent(ButtonObject, EventTriggerType.PointerClick, Callback);
            Tools.BindEvent(ButtonObject, EventTriggerType.Scroll, MainGame.Instance.Descriptor.OnTextScroll);
        }

        public void Set(string text, int response, Transform parent)
        {
            ButtonObject.transform.parent = parent;
            ButtonObject.transform.GetChild(0).GetComponent<TextMeshPro>().text = text;
            this.response = response;
            ButtonObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        public void AddOption(Tuple<int, int> option)
        {
            if (activateOptions == null)
            {
                activateOptions = new List<Tuple<int, int>>();
            }
            activateOptions.Add(option);
        }

        public void Show()
        {
            ButtonObject.transform.localScale = Vector3.one;
        }

        public void Hide()
        {
            ButtonObject.transform.localScale = Vector3.zero;
        }

        public void Disable()
        {
            ButtonObject.transform.parent = null;
            ButtonObject.transform.localScale = Vector3.zero;
        }

        public void Reset()
        {
            ButtonObject.transform.localScale = Vector3.zero;
            activateOptions?.Clear();
        }

        public void Callback(BaseEventData data)
        {
            if (activateOptions != null && activateOptions.Count > 1)
            {
                MainGame.Instance.OptionSelector.SelectOptions(activateOptions);
            }
            else
            {
                MainGame.Instance.SendCToSResponse(response);
            }
        }
    }
}
