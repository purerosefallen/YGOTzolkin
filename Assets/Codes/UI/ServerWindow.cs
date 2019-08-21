using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YGOTzolkin.Service;
using YGOTzolkin.Utility;

namespace YGOTzolkin.UI
{
    class ServerWindow : WindowBase
    {
        private readonly TMP_InputField iptName;
        private readonly TMP_InputField iptIp;
        private readonly TMP_InputField iptPort;
        private readonly TMP_InputField iptPassword;
        private readonly Button btnQuit;
        private readonly Button btnJoin;

        private TMP_Dropdown dpdHosts;
        private TMP_Dropdown dpdNames;
        private TMP_Dropdown dpdPwds;
        private TMP_Dropdown dpdPorts;

        private List<string> hosts;
        private List<string> names;
        private List<string> ports;
        private List<string> passwords;

        public ServerWindow()
        {
            MainCanvas = Tools.LoadResource<Canvas>("Prefabs/ServerWindow");
            Hide();
            var txthost = GetControl<TextMeshProUGUI>("txtip");
            txthost.text = DataService.TzlString(9);
            var txtport = GetControl<TextMeshProUGUI>("txtport");
            txtport.text = DataService.TzlString(10);
            var txtpwd = GetControl<TextMeshProUGUI>("txtpassword");
            txtpwd.text = DataService.SysString(1222);
            var txtname = GetControl<TextMeshProUGUI>("txtname");
            txtname.text = DataService.SysString(1220);

            TryGetControl(out iptName, "IptName");
            TryGetControl(out iptIp, "IptIp");
            TryGetControl(out iptPort, "IptPort");
            TryGetControl(out iptPassword, "IptPassword");
            TryGetControl(out btnQuit, "BtnQuit");
            TryGetControl(out btnJoin, "BtnJoin");
            iptName.onSubmit.AddListener(OnSummit);
            iptIp.onSubmit.AddListener(OnSummit);
            iptPort.onSubmit.AddListener(OnSummit);
            iptPassword.onSubmit.AddListener(OnSummit);

            btnQuit.onClick.AddListener(OnExitDeck);
            btnQuit.SetButtonName(DataService.SysString(1210));
            btnJoin.onClick.AddListener(OnJoin);
            btnJoin.SetButtonName(DataService.SysString(1223));

            TryGetControl(out dpdHosts, nameof(dpdHosts));
            TryGetControl(out dpdPorts, nameof(dpdPorts));
            TryGetControl(out dpdPwds, nameof(dpdPwds));
            TryGetControl(out dpdNames, nameof(dpdNames));
            hosts = new List<string>(Config.GetString("CachedHosts", string.Empty).Split(new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries));
            foreach (var h in hosts)
            {
                dpdHosts.options.Add(new TMP_Dropdown.OptionData(h));
            }
            ports = new List<string>(Config.GetString("CachedPorts", string.Empty).Split(new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries));
            foreach (var p in ports)
            {
                dpdPorts.options.Add(new TMP_Dropdown.OptionData(p.ToString()));
            }
            names = new List<string>(Config.GetString("CachedNames", string.Empty).Split(new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries));
            foreach (var n in names)
            {
                dpdNames.options.Add(new TMP_Dropdown.OptionData(n));
            }
            dpdNames.onValueChanged.AddListener(OnCachedNames);
        }

        internal override void Show()
        {
            iptIp.text = Config.GetString("Server", "127.0.0.1");
            iptPort.text = Config.GetString("Port", "7911");
            iptPassword.text = Config.GetString("Password", "test");
            iptName.text = Config.GetString("PlayerName", "Ignistar");
            base.Show();
        }

        private void OnSummit(string input)
        {
            OnJoin();
        }

        private void OnExitDeck()
        {
            Hide();
            MainGame.Instance.Menu.Show();
        }

        private void OnCachedNames(int i)
        {
            if (i < dpdNames.options.Count)
            {
                iptName.text = dpdNames.options[i].text;
            }
        }

        private void OnJoin()
        {
            if (NetworkService.Instance.Connected)
            {
                return;
            }
            if (string.IsNullOrEmpty(iptName.text))
            {
                MainGame.Instance.HintBox.ShowHint(DataService.TzlString(2), 1);
                return;
            }
            if (string.IsNullOrEmpty(iptIp.text))
            {
                MainGame.Instance.HintBox.ShowHint(DataService.TzlString(3), 1);
                return;
            }
            if (string.IsNullOrEmpty(iptPort.text))
            {
                MainGame.Instance.HintBox.ShowHint(DataService.TzlString(4), 1);
                return;
            }
            if (!int.TryParse(iptPort.text, out int port))
            {
                MainGame.Instance.HintBox.ShowHint(DataService.TzlString(5), 1);
                return;
            }
            if (!NetworkService.Instance.JoinServer(iptIp.text, port, iptName.text, iptPassword.text))
            {
                MainGame.Instance.HintBox.ShowHint(DataService.SysString(1400), 1f);
            }
            Config.Set("Server", iptIp.text);
            Config.Set("Port", iptPort.text);
            Config.Set("Password", iptPassword.text);
            Config.Set("PlayerName", iptName.text);
            GameInfo.Instance.LocalName = iptName.text;
            if (dpdNames.options.Find((op) => op.text == iptName.text) == null)
            {
                dpdNames.options.Add(new TMP_Dropdown.OptionData(iptName.text));
            }
        }
    }
}

