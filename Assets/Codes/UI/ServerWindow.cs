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

        //todo load cached config

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
        }
    }
}

