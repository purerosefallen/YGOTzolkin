using UnityEngine;
using UnityEngine.UI;
using YGOTzolkin.Service;


namespace YGOTzolkin.UI
{
    class ConfigWindow : LazyWindow
    {
        private Toggle tglAutoMP;
        private Toggle tglAutoSP;
        private Toggle tglRandomP;

        public ConfigWindow()
        {
           
        }

        protected override void LazyInitialize()
        {
            MainCanvas = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/ConfigWindow") as GameObject).GetComponent<Canvas>();
            TryGetControl(out tglAutoMP, "TglAutoMP");
            TryGetControl(out tglAutoSP, "TglAutoSP");
            TryGetControl(out tglRandomP, "TglRandomP");
            TryGetControl(out Button exit, "BtnClose");
            exit.onClick.AddListener(Hide);

            tglAutoMP.transform.GetChild(1).GetComponent<Text>().text = DataService.SysString(1274);
            tglAutoMP.isOn = Config.GetBool("AutoMPlace");
            tglAutoMP.onValueChanged.AddListener(OnValueChanged);
            tglAutoSP.transform.GetChild(1).GetComponent<Text>().text = DataService.SysString(1278);
            tglAutoSP.isOn = Config.GetBool("AutoSPlace");
            tglAutoSP.onValueChanged.AddListener(OnValueChanged);
            tglRandomP.transform.GetChild(1).GetComponent<Text>().text = DataService.SysString(1275).Substring(1);
            tglRandomP.isOn = Config.GetBool("RandomPlace");
            tglRandomP.onValueChanged.AddListener(OnValueChanged);
            if (!tglAutoMP.isOn && !tglAutoSP.isOn)
            {
                tglRandomP.enabled = false;
            }
            else
            {
                tglRandomP.enabled = true;
            }
        }

        private void OnValueChanged(bool ph)
        {
            Config.Set("AutoSPlace", tglAutoSP.isOn.ToString());
            Config.Set("AutoMPlace", tglAutoMP.isOn.ToString());
            Config.Set("RandomPlace", tglRandomP.isOn.ToString());
            if (!tglAutoMP.isOn && !tglAutoSP.isOn)
            {
                tglRandomP.enabled = false;
            }
            else
            {
                tglRandomP.enabled = true;
            }
        }
    }
}
