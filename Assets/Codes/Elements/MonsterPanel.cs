using TMPro;
using UnityEngine;
using YGOTzolkin.Utility;

namespace YGOTzolkin.Elements
{
    class MonsterPanel : IPoolSupporter
    {
        public GameObject PanelObjecet;
        public TextMeshPro TextAtk;
        public TextMeshPro TextDef;
        public TextMeshPro TextLevel;
        public GameObject StarObject;

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
