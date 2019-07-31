using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YGOTzolkin.Service;
using YGOTzolkin.Utility;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.UI
{
    class RoomWindow : WindowBase
    {
        private Button btnReady;
        private Button btnQuit;
        private Button btnNotReady;
        private Button btnStart;
        private Button btnToDuelist;
        private Button btnToWatcher;
        private TextMeshProUGUI tMPDuelInfo;
        private TextMeshProUGUI tMPWatcherCount;
        private TextMeshProUGUI tMPSelectedDeck;

        private RectTransform decksContent;
        private RectTransform RectDuelists;

        private readonly List<Toggle> duelistToggles;
        private readonly List<TextMeshProUGUI> duelistNames;
        private readonly List<Button> duelistKicks;
        private readonly List<Transform> duelistsTransforms;
        private readonly List<Transform> deckItems;

        public RoomWindow()
        {
            MainCanvas = Tools.LoadResource<Canvas>("Prefabs/RoomWindow");
            MainCanvas.gameObject.SetActive(false);
            IsShowing = false;
            deckItems = new List<Transform>();
            duelistToggles = new List<Toggle>(4);
            duelistKicks = new List<Button>(4);
            duelistNames = new List<TextMeshProUGUI>(4);
            duelistsTransforms = new List<Transform>(4);

            btnQuit = GetControl<Button>("BtnQuit");
            btnReady = GetControl<Button>("BtnReady");
            btnNotReady = GetControl<Button>("BtnNotReady");
            btnStart = GetControl<Button>("BtnStart");
            btnToDuelist = GetControl<Button>("BtnToDuelist");
            btnToWatcher = GetControl<Button>("BtnToWatcher");
            btnQuit.SetButtonName(DataService.SysString(1210));
            btnReady.SetButtonName(DataService.SysString(1218));
            btnNotReady.SetButtonName(DataService.SysString(1219));
            btnStart.SetButtonName(DataService.SysString(1215));
            btnToDuelist.SetButtonName(DataService.SysString(1251));
            btnToWatcher.SetButtonName(DataService.SysString(1252));
            btnQuit.onClick.AddListener(OnExitClick);
            btnNotReady.onClick.AddListener(OnNotReadyClick);
            btnReady.onClick.AddListener(OnReadyClick);
            btnToDuelist.onClick.AddListener(OnToDuelistClick);
            btnStart.onClick.AddListener(OnStartClick);
            btnToWatcher.onClick.AddListener(OnToWatcherClick);

            tMPDuelInfo = GetControl<TextMeshProUGUI>("TMPDuelInfo");
            tMPWatcherCount = GetControl<TextMeshProUGUI>("TMPWatcherCount");
            tMPSelectedDeck = GetControl<TextMeshProUGUI>("TMPSelectedDeck");

            decksContent = GetControl<RectTransform>("DecksContent");

            RectDuelists = GetControl<RectTransform>("Duelists");
            for (int i = 0; i < 4; ++i)
            {
                var dl = GetControl<Transform>("Duelist" + i.ToString());
                duelistsTransforms.Add(dl);
                duelistKicks.Add(dl.Find("Kick").GetComponent<Button>());
                int pos = i;
                duelistKicks[i].onClick.AddListener(() => OnKickClick(pos));
                duelistToggles.Add(dl.Find("Toggle").GetComponent<Toggle>());
                duelistNames.Add(duelistToggles[i].transform.Find("Label").GetComponent<TextMeshProUGUI>());
            }
        }

        public void Show(string duelInfo)
        {
            MainGame.Instance.ChatWindow.Show();
            RefreshDeckList();
            tMPDuelInfo.text = duelInfo;
            if (GameInfo.Instance.IsTag)
            {
                for (int i = 0; i < 4; ++i)
                {
                    duelistsTransforms[i].parent = RectDuelists;
                    duelistNames[i].text = "";
                    duelistToggles[i].isOn = false;
                }
            }
            else
            {
                for (int i = 0; i < 2; ++i)
                {
                    duelistsTransforms[i].parent = RectDuelists;
                    duelistNames[i].text = "";
                    duelistToggles[i].isOn = false;
                }
                duelistsTransforms[3].parent = null;
                duelistsTransforms[2].parent = null;
            }
            SetVisible(btnReady.transform, true);
            SetVisible(btnNotReady.transform, false);
            UpdateWatcherCount(0);
            Show();
        }

        private void RefreshDeckList()
        {
            var allnames = DeckService.GetAllNames();
            if (File.Exists(Config.DeckPath + Config.GetString("LastDeck", "") + ".ydk"))
            {
                tMPSelectedDeck.text = Config.GetString("LastDeck");
            }
            else if (allnames.Count > 0)
            {
                tMPSelectedDeck.text = allnames[0];
                Config.Set("LastDeck", tMPSelectedDeck.text);
            }
            var itemstack = new Stack<Transform>(deckItems.Count);
            for (int i = 0; i < deckItems.Count; ++i)
            {
                deckItems[i].SetParent(null);
                itemstack.Push(deckItems[i]);
            }
            deckItems.Clear();
            for (int i = 0; i < allnames.Count; ++i)
            {
                var item = itemstack.Count > 0 ? itemstack.Pop() : Tools.LoadResource<RectTransform>("Prefabs/DeckItem");
                var tmp = item.GetChild(2).GetComponent<TextMeshProUGUI>();
                var itemtgl = item.GetComponent<Toggle>();
                tmp.text = allnames[i];
                item.SetParent(decksContent, false);
                itemtgl.group = decksContent.GetComponent<ToggleGroup>();
                item.localScale = Vector3.one;
                itemtgl.isOn = false;
                itemtgl.enabled = true;
                itemtgl.onValueChanged.RemoveAllListeners();
                itemtgl.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        tMPSelectedDeck.text = tmp.text;
                    }
                });
                deckItems.Add(item);
            }
            while (itemstack.Count > 0)
            {
                UnityEngine.Object.Destroy(itemstack.Pop());
            }
        }

        public void PlayerEnter(byte position, string name)
        {
            if (position > 3)
            {
                return;
            }
            duelistNames[position].text = name;
            if (GameInfo.Instance.IsTag)
            {
                GameInfo.Instance.HostName = duelistNames[0].text;
                GameInfo.Instance.HostNameTag = duelistNames[1].text;
                GameInfo.Instance.ClientName = duelistNames[2].text;
                GameInfo.Instance.ClientNameTag = duelistNames[3].text;
            }
            else
            {
                GameInfo.Instance.HostName = duelistNames[0].text;
                GameInfo.Instance.ClientName = duelistNames[1].text;
            }
        }

        public void PlayerChange(int position, int state)
        {
            if (position > 3)
            {
                return;
            }
            switch (state)
            {
                case (int)PlayerState.Ready:
                    duelistToggles[position].isOn = true;
                    if (position == GameInfo.Instance.SelfType)
                    {
                        SetVisible(btnNotReady.transform, true);
                        SetVisible(btnReady.transform, false);
                    }
                    break;
                case (int)PlayerState.NotReady:
                    duelistToggles[position].isOn = false;
                    if (position == GameInfo.Instance.SelfType)
                    {
                        SetVisible(btnNotReady.transform, false);
                        SetVisible(btnReady.transform, true);
                    }
                    break;
                case (int)PlayerState.Leave:
                    duelistToggles[position].isOn = false;
                    duelistNames[position].text = "";
                    break;
                case (int)PlayerState.Observe:
                    GameInfo.Instance.WatchingCount++;
                    tMPWatcherCount.text = DataService.SysString(1253) + GameInfo.Instance.WatchingCount.ToString();
                    duelistToggles[position].isOn = false;
                    duelistNames[position].text = "";
                    break;
                default:
                    if (state < 8)
                    {
                        string prename = duelistNames[position].text;
                        duelistNames[state].text = prename;
                        duelistToggles[state].isOn = false;
                        duelistNames[position].text = "";
                        duelistToggles[position].isOn = false;
                    }
                    break;
            }
            if (duelistToggles[0].isOn && duelistToggles[1].isOn
                && (!GameInfo.Instance.IsTag || (duelistToggles[2].isOn && duelistToggles[3].isOn)))
            {
                btnStart.enabled = true;
            }
            else
            {
                btnStart.enabled = false;
            }
        }

        internal void ChangeType()
        {
            SetVisible(btnStart.transform, GameInfo.Instance.IsHost);
            if (!GameInfo.Instance.IsTag)
            {
                SetVisible(duelistKicks[0].transform, GameInfo.Instance.IsHost);
                SetVisible(duelistKicks[1].transform, GameInfo.Instance.IsHost);
                duelistToggles[0].isOn = false;
                duelistToggles[1].isOn = false;
                if (GameInfo.Instance.SelfType < 2)
                {
                    duelistToggles[GameInfo.Instance.SelfType].isOn = false;
                    SetVisible(btnToDuelist.transform, false);
                    SetVisible(btnToWatcher.transform, true);
                    SetVisible(btnReady.transform, true);
                    SetVisible(btnNotReady.transform, false);
                }
                else
                {
                    SetVisible(btnToDuelist.transform, true);
                    SetVisible(btnToWatcher.transform, false);
                    SetVisible(btnReady.transform, false);
                    SetVisible(btnNotReady.transform, false);
                }
            }
            else
            {
                SetVisible(btnToDuelist.transform, true);
                for (int i = 0; i < 4; ++i)
                {
                    SetVisible(duelistKicks[i].transform, GameInfo.Instance.IsHost);
                }
                if (GameInfo.Instance.SelfType < 4)
                {
                    duelistToggles[GameInfo.Instance.SelfType].isOn = false;
                    SetVisible(btnToWatcher.transform, true);
                    SetVisible(btnReady.transform, true);
                    SetVisible(btnNotReady.transform, false);
                }
                else
                {
                    SetVisible(btnToWatcher.transform, false);
                    SetVisible(btnReady.transform, false);
                    SetVisible(btnNotReady.transform, false);
                }
                if (duelistToggles[0].isOn && duelistToggles[1].isOn && duelistToggles[2].isOn && duelistToggles[3].isOn)
                {
                    btnStart.enabled = true;
                }
                else
                {
                    btnStart.enabled = false;
                }
            }
            GameInfo.Instance.PlayerType = (PlayerType)GameInfo.Instance.SelfType;
        }

        internal void UpdateTimeLimit()
        {
        }

        internal void UpdateWatcherCount(ushort watchCount)
        {
            GameInfo.Instance.WatchingCount = watchCount;
            tMPWatcherCount.text = DataService.SysString(1253) + watchCount.ToString();
        }

        internal void SetDeckListEnable(bool enabled)
        {
            foreach (var item in deckItems)
            {
                item.GetComponent<Toggle>().enabled = enabled;
            }
        }

        internal void ExtractNames()
        {
            if (!GameInfo.Instance.IsTag)
            {
                if (GameInfo.Instance.SelfType != 1)
                {
                    GameInfo.Instance.HostName = duelistNames[0].text;
                    GameInfo.Instance.ClientName = duelistNames[1].text;
                }
                else
                {
                    GameInfo.Instance.HostName = duelistNames[1].text;
                    GameInfo.Instance.ClientName = duelistNames[0].text;
                }
            }
            else
            {
                if (GameInfo.Instance.SelfType < 4)
                {
                    if (GameInfo.Instance.SelfType > 1)
                    {
                        GameInfo.Instance.HostName = duelistNames[2].text;
                        GameInfo.Instance.HostNameTag = duelistNames[3].text;
                        GameInfo.Instance.ClientName = duelistNames[0].text;
                        GameInfo.Instance.ClientNameTag = duelistNames[1].text;
                    }
                    else
                    {
                        GameInfo.Instance.HostName = duelistNames[0].text;
                        GameInfo.Instance.HostNameTag = duelistNames[1].text;
                        GameInfo.Instance.ClientName = duelistNames[2].text;
                        GameInfo.Instance.ClientNameTag = duelistNames[3].text;
                    }
                }
            }
        }
        #region events
        private void OnExitClick()
        {
            MainGame.Instance.ChatWindow.Hide();
            NetworkService.Instance.Disconnect();
            MainGame.Instance.ServerWindow.Show();
            MainGame.Instance.ToolStrip.Hide();
            Hide();
        }

        private void OnToWatcherClick()
        {
            NetworkService.Instance.Send(CToSMessage.HsToObserver);
        }

        private void OnToDuelistClick()
        {
            SetDeckListEnable(true);
            NetworkService.Instance.Send(CToSMessage.HsToDuelist);
        }

        private void OnReadyClick()
        {
            string fname = Config.DeckPath + tMPSelectedDeck.text + ".ydk";
            if (!File.Exists(fname))
            {
                MainGame.Instance.HintBox.ShowHint(DataService.TzlString(6), 1f);
                RefreshDeckList();
                return;
            }
            DeckService.Load(fname);
            Config.Set("LastDeck", tMPSelectedDeck.text);
            SetDeckListEnable(false);
            NetworkService.Instance.Send(DeckService.CurrentDeck.ToArray());
            NetworkService.Instance.Send(CToSMessage.HsReady);
        }

        private void OnNotReadyClick()
        {
            NetworkService.Instance.Send(CToSMessage.HsNotReady);
            SetDeckListEnable(true);
        }

        private void OnStartClick()
        {
            NetworkService.Instance.Send(CToSMessage.HsStart);
        }

        private void OnKickClick(int position)
        {
            NetworkService.Instance.Send(new byte[]
            {
                0x02,
                0x00,
                (byte)CToSMessage.HsKick,
                (byte)position,
            });
        }
        #endregion
    }
}
