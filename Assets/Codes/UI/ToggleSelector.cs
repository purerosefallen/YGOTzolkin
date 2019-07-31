using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YGOTzolkin.Service;

namespace YGOTzolkin.UI
{
    class ToggleSelector : WindowBase
    {

        private int selectedValue;
        private int selectionCount;
        private int selectedCount;
        private readonly List<Toggle> toggles;

        private readonly RectTransform mainRect;

        public ToggleSelector()
        {
            MainCanvas = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/ToggleSelector") as GameObject).GetComponent<Canvas>();
            TryGetControl(out mainRect, "Panel");
            toggles = new List<Toggle>();
            selectedValue = 0;
            selectedCount = 0;
        }

        internal override void Hide()
        {
            base.Hide();
            if (toggles.Count > 0)
            {
                foreach (var t in toggles)
                {
                    t.transform.SetParent(null);
                    UnityEngine.Object.Destroy(t);
                }
                toggles.Clear();
                selectedValue = 0;
                selectionCount = 0;
                selectedCount = 0;
            }
        }

        private Toggle AddToggle(string text)
        {
            var tgl = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/OptionToggle") as GameObject).GetComponent<Toggle>();
            toggles.Add(tgl);
            tgl.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = text;
            tgl.transform.SetParent(mainRect, false);
            tgl.transform.localScale = Vector3.one;
            return tgl;
        }

        internal void SelectNumber(List<int> numbers)
        {
            for (int i = 0; i < numbers.Count; ++i)
            {
                var tgl = AddToggle(numbers[i].ToString());
                int response = i;
                tgl.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        MainGame.Instance.SendCToSResponse(response);
                        Hide();
                    }
                });
            }
            Show();
        }

        internal void SelectRace(int count, int available)
        {
            selectionCount = count;

            for (int i = 0, filter = 0x1; i < 25; ++i, filter <<= 1)
            {
                if ((filter & available) == 0)
                {
                    continue;
                }
                int rv = filter;
                var tgl = AddToggle(DataService.SysString((uint)i + 1020));
                tgl.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        selectedValue |= rv;
                        selectedCount++;
                    }
                    else
                    {
                        selectedValue &= rv;
                        selectedCount--;
                    }
                    if (selectedCount == selectionCount)
                    {
                        MainGame.Instance.SendCToSResponse(selectedValue);
                        Hide();
                    }
                });
            }
            if (toggles.Count == selectionCount/*&& auto select is on*/)
            {
                MainGame.Instance.SendCToSResponse(available);
                Hide();
                return;
            }
            Show();
        }

        internal void SelectAttribute(int count, int available)
        {
            selectionCount = count;
            for (int i = 0, filter = 0x1; i < 7; ++i, filter <<= 1)
            {
                if ((filter & available) == 0)
                {
                    continue;
                }
                int rv = filter;
                var tgl = AddToggle(DataService.SysString((uint)i + 1010));
                tgl.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        selectedValue |= rv;
                        selectedCount++;
                    }
                    else
                    {
                        selectedValue &= rv;
                        selectedCount--;
                    }
                    if (selectedCount == selectionCount)
                    {
                        MainGame.Instance.SendCToSResponse(selectedValue);
                        Hide();
                    }
                });
            }
            if (toggles.Count == selectionCount&&Config.GetBool("AutoSelection",true))
            {
                MainGame.Instance.SendCToSResponse(available);
                Hide();
                return;
            }
            Show();
        }
    }
}
