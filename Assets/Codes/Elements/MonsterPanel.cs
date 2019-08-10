using TMPro;
using UnityEngine;
using YGOTzolkin.Utility;

namespace YGOTzolkin.Elements
{
    class MonsterPanel : IPoolSupporter
    {
        public GameObject PanelObjecet { get; private set; }
        public TextMeshPro TextAtk { get; private set; }
        public TextMeshPro TextDef { get; private set; }
        public TextMeshPro TextLevel { get; private set; }
        public GameObject StarObject { get; private set; }

        public MonsterPanel()
        {
            PanelObjecet = MonoBehaviour.Instantiate(Resources.Load("Prefabs/MonsterPanel") as GameObject);
            TextAtk = PanelObjecet.transform.GetChild(0).GetComponent<TextMeshPro>();
            TextDef = PanelObjecet.transform.GetChild(1).GetComponent<TextMeshPro>();
            TextLevel = PanelObjecet.transform.GetChild(2).GetComponent<TextMeshPro>();
            StarObject = PanelObjecet.transform.GetChild(3).gameObject;
        }

        public void Disable()
        {
            PanelObjecet.transform.localScale = Vector3.zero;
        }

        public void Reset()
        {
            PanelObjecet.transform.localScale = Vector3.one;
        }

    }
}
