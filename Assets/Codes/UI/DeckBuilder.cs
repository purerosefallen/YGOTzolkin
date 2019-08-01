using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YGOTzolkin.Elements;
using YGOTzolkin.Service;
using YGOTzolkin.Utility;
using YGOTzolkin.YGOModel;
using static YGOTzolkin.Service.DataService;

namespace YGOTzolkin.UI
{
    class DeckBuilder : WindowBase
    {
        internal ForbiddenList CurrentFList { get; set; }
        internal bool IsSideChanging { get; private set; }

        private int preMainCount;
        private int preExtraCount;

        private readonly ObjectPool<BuildCard> cardPool;
        private readonly List<BuildCard> mainCards;
        private readonly List<BuildCard> extraCards;
        private readonly List<BuildCard> sideCards;
        private readonly List<List<BuildCard>> allCards;
        private bool deckModified;
        private string deckPath;
        private readonly float tableWidth;
        private readonly float mainTop;
        private readonly float mainLeft;
        private readonly float mainVerticalStride;
        private readonly float extraTop;
        private readonly float sideTop;
        private readonly float cardHeight;
        private readonly float cardWidth;

        private CardExplorer explorer;
        private TMP_Dropdown dpdDecks;
        private TMP_Dropdown dpdFList;
        private Button btnFinishSide;
        private Button btnShuffle;
        private Button btnRename;
        private Button btnSort;
        private Button btnClear;
        private Button btnExit;
        private Button btnCopy;
        private Button btnSave;
        private Button btnDelete;

        private TextMeshPro MainCount;
        private TextMeshPro ExtraCount;
        private TextMeshPro SideCount;

        public DeckBuilder()
        {
            cardHeight = 8.6f;
            cardWidth = 5.9f;
            mainTop = 18;
            mainLeft = -28;
            tableWidth = mainLeft * -2;
            mainVerticalStride = 8.7f;
            extraTop = mainTop - 3 * mainVerticalStride - 9;
            sideTop = extraTop - 9;

            mainCards = new List<BuildCard>();
            extraCards = new List<BuildCard>();
            sideCards = new List<BuildCard>();
            allCards = new List<List<BuildCard>>
            {
                mainCards,
                extraCards,
                sideCards,
            };
            cardPool = new ObjectPool<BuildCard>();
            deckPath = string.Empty;

            MainCanvas = Tools.LoadResource<Canvas>("Prefabs/DeckBuilder");
            MainCanvas.gameObject.SetActive(false);
            TryGetControl(out dpdDecks, "DpdDecks");
            dpdDecks.onValueChanged.AddListener(OnDeckChanged);
            TryGetControl(out dpdFList, "DpdFList");
            dpdFList.onValueChanged.AddListener(OnFListChanged);
            explorer = new CardExplorer();
            btnShuffle = GetControl<Button>("BtnShuffle");
            btnShuffle.onClick.AddListener(OnShuffle);
            btnSort = GetControl<Button>("BtnSort");
            btnSort.onClick.AddListener(OnSort);
            btnClear = GetControl<Button>("BtnClear");
            btnClear.onClick.AddListener(OnClear);
            btnCopy = GetControl<Button>("BtnCopy");
            btnCopy.onClick.AddListener(OnSaveAs);
            btnRename = GetControl<Button>("BtnRename");
            btnRename.onClick.AddListener(OnRename);
            btnDelete = GetControl<Button>("BtnDelete");
            btnDelete.onClick.AddListener(OnDelete);
            btnSave = GetControl<Button>("BtnSave");
            btnSave.onClick.AddListener(OnSave);
            btnExit = GetControl<Button>("BtnExit");
            btnExit.onClick.AddListener(OnExit);
            TryGetControl(out btnFinishSide, nameof(btnFinishSide));
            btnFinishSide.onClick.AddListener(OnFinishSide);

            MainCount = Tools.LoadResource<TextMeshPro>("Prefabs/BuilderGroupCount");
            MainCount.transform.position = new Vector3(37.5f, 0, mainTop);
            MainCount.gameObject.SetActive(false);
            ExtraCount = Tools.LoadResource<TextMeshPro>("Prefabs/BuilderGroupCount");
            ExtraCount.transform.position = new Vector3(37.5f, 0, extraTop);
            ExtraCount.gameObject.SetActive(false);
            SideCount = Tools.LoadResource<TextMeshPro>("Prefabs/BuilderGroupCount");
            SideCount.transform.position = new Vector3(37.5f, 0, sideTop);
            SideCount.gameObject.SetActive(false);
        }

        internal override void Show()
        {
            base.Show();
            IsSideChanging = false;
            explorer.Show();
            SetVisible(dpdDecks.transform, true);
            SetVisible(dpdFList.transform, true);
            SetVisible(btnFinishSide.transform, false);
            SetVisible(btnClear.transform, true);
            SetVisible(btnCopy.transform, true);
            SetVisible(btnDelete.transform, true);
            SetVisible(btnExit.transform, true);
            SetVisible(btnRename.transform, true);
            SetVisible(btnSave.transform, true);
            DeckService.ForbiddenLists.ForEach((fl) => dpdFList.options.Add(new TMP_Dropdown.OptionData(fl.Name)));
            dpdFList.value = 0;
            dpdFList.RefreshShownValue();
            CurrentFList = DeckService.ForbiddenLists[0];
            var names = DeckService.GetAllNames();
            if (names.Count == 0)
            {
                File.Create(Config.DeckPath + "new deck.ydk");
                names.Add("new deck.ydk");
            }
            bool found = false;
            for (int i = 0; i < names.Count; ++i)
            {
                dpdDecks.options.Add(new TMP_Dropdown.OptionData(names[i]));
                if (Config.GetString("LastDeck", "") == names[i])
                {
                    found = i != dpdDecks.value;
                    dpdDecks.value = i;
                }
            }
            dpdDecks.RefreshShownValue();
            if (!found)
            {
                SwitchToDeck(Config.DeckPath + dpdDecks.options[dpdDecks.value].text + ".ydk");
            }
            MainCount.gameObject.SetActive(true);
            ExtraCount.gameObject.SetActive(true);
            SideCount.gameObject.SetActive(true);
            MainGame.Instance.CameraToBuilder();
            MainGame.Instance.Descriptor.Show();
            MainGame.Instance.FrameActions.Add(FocusInput);
        }

        internal override void Hide()
        {
            base.Hide();
            deckModified = false;
            if (!IsSideChanging)
            {
                MainGame.Instance.Descriptor.Hide();
                MainGame.Instance.Menu.Show();
                dpdDecks.ClearOptions();
                explorer.Hide();
            }
            MainGame.Instance.FrameActions.Remove(FocusInput);
            ClearDeck();
            MainCount.gameObject.SetActive(false);
            ExtraCount.gameObject.SetActive(false);
            SideCount.gameObject.SetActive(false);
        }

        internal void ShowSideChange()
        {
            base.Show();
            IsSideChanging = true;
            MainGame.Instance.CameraToBuilder();
            MainGame.Instance.Descriptor.Show();
            SetVisible(dpdDecks.transform, false);
            SetVisible(dpdFList.transform, false);
            SetVisible(btnFinishSide.transform, true);
            SetVisible(btnClear.transform, false);
            SetVisible(btnCopy.transform, false);
            SetVisible(btnDelete.transform, false);
            SetVisible(btnExit.transform, false);
            SetVisible(btnRename.transform, false);
            SetVisible(btnSave.transform, false);
            for (int i = 0; i < 3; ++i)
            {
                foreach (var code in DeckService.CurrentDeck.AllCards[i])
                {
                    AddNewCard(GetCardData(code), i);
                }
                SpreadCard(i);
            }
            preMainCount = DeckService.CurrentDeck.Main.Count;
            preExtraCount = DeckService.CurrentDeck.Extra.Count;
            MainCount.gameObject.SetActive(true);
            ExtraCount.gameObject.SetActive(true);
            SideCount.gameObject.SetActive(true);
        }

        internal void PushCard(BuildCard card)
        {
            int originalGroup = allCards.FindIndex((list) => list.Find(card.Equals) != null);
            if (originalGroup == -1)
            {
                return;
            }
            int tarGroup = GetHoverdGroup();
            if (tarGroup == -1)
            {
                RemoveCard(card, originalGroup);
            }
            else
            {
                if (!(card.Data.IsExtra() && tarGroup == 0 || !card.Data.IsExtra() && tarGroup == 1 || tarGroup == originalGroup))
                {
                    allCards[originalGroup].Remove(card);
                    allCards[tarGroup].Add(card);
                    allCards[tarGroup].Sort(BuildCard.CompareByPosition);
                    SpreadCard(tarGroup);
                }
                deckModified = true;
                allCards[originalGroup].Sort(BuildCard.CompareByPosition);
                SpreadCard(originalGroup);
            }
        }

        internal void RemoveCard(BuildCard card, int group = -1)
        {
            if (group < 0)
            {
                group = GetHoverdGroup();
            }
            deckModified = true;
            allCards[group].Remove(card);
            cardPool.Store(card);
            SpreadCard(group);
        }

        internal void CopyCard(BuildCard card)
        {
            int group = GetHoverdGroup();
            if (group < 0 || IsSideChanging || !CanCopyCard(card.Data))
            {
                return;
            }
            BuildCard copy = AddNewCard(card.Data, group);
            copy.Transform.position = card.Transform.position;
            deckModified = true;
            allCards[group].Sort(BuildCard.CompareByPosition);
            SpreadCard(group);
        }

        internal BuildCard CopyCard(CardData data, Vector2 position, bool dragging = false)
        {
            if (!CanCopyCard(data) || IsSideChanging)
            {
                return null;
            }
            int targroup = data.IsExtra() ? 1 : 0;
            BuildCard copy = AddNewCard(data, targroup);
            copy.Transform.position = MainGame.Instance.MainCamera.ScreenToWorldPoint(new Vector3(position.x, position.y, 95));
            if (!dragging)
            {
                deckModified = true;
                allCards[targroup].Sort(BuildCard.CompareByPosition);
                SpreadCard(targroup);
            }
            return copy;
        }

        internal BuildCard AddNewCard(CardData data, int group)
        {
            var card = cardPool.New();
            card.SetData(data);
            allCards[group].Add(card);
            return card;
        }

        private void FocusInput()
        {
            if (Input.GetKeyDown(KeyCode.Return) && !IsSideChanging)
            {
                explorer.FocusInput();
            }
        }

        private bool CanCopyCard(CardData data)
        {
            int count = 0;
            uint alias = data.Alias;
            foreach (var list in allCards)
            {
                count += list.Count((card) => data.Code == card.Data.Code || data.Code == card.Data.Alias ||
                        (alias != 0 && (alias == card.Data.Alias || alias == card.Data.Code)));
            }
            return (count < CurrentFList.Query(data));
        }

        private int GetHoverdGroup()
        {
            var position = MainGame.Instance.MainCamera.ScreenToWorldPoint(
              new Vector3(Input.mousePosition.x, Input.mousePosition.y, MainGame.Instance.MainCamera.transform.position.y));
            float posx = position.x;
            float posy = position.z;
            if (Mathf.Abs(posx) > mainLeft * -1 + cardWidth || posy > mainTop + cardHeight || posy < sideTop - cardHeight)
            {
                return -1;
            }
            else
            {
                if (posy > extraTop + 0.5f * cardHeight)
                {
                    return 0;
                }
                else if (posy < sideTop + 0.5f * cardHeight)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
        }

        private void SwitchToDeck(string newPath)
        {
            deckModified = false;
            deckPath = newPath;
            DeckService.Load(deckPath);
            ClearDeck();
            for (int i = 0; i < 3; ++i)
            {
                foreach (var code in DeckService.CurrentDeck.AllCards[i])
                {
                    AddNewCard(GetCardData(code), i);
                }
                SpreadCard(i);
            }
        }

        private void ClearDeck()
        {
            foreach (var list in allCards)
            {
                cardPool.Store(list);
                list.Clear();
            }
            MainCount.text = 0.ToString();
            SideCount.text = 0.ToString();
            ExtraCount.text = 0.ToString();
        }

        private void SpreadCard(int group)
        {
            if (group == 0)
            {
                int rowRemainder = mainCards.Count > 40 ? mainCards.Count % 4 : 0;
                int columnSize = mainCards.Count > 40 ? mainCards.Count / 4 : 10;
                int cardIndex = 0, rowSize;
                for (int i = 0; i < 4; ++i, --rowRemainder)
                {
                    rowSize = columnSize + (rowRemainder > 0 ? 1 : 0);
                    float stride = tableWidth / (rowSize - 1);
                    for (int j = 0; j < rowSize && cardIndex < mainCards.Count; ++j, ++cardIndex)
                    {
                        mainCards[cardIndex].MoveTo(new Vector3(mainLeft + stride * j, 0, mainTop - mainVerticalStride * i));
                    }
                }
                MainCount.text = mainCards.Count.ToString();
            }
            else
            {
                int rowSize = allCards[group].Count > 9 ? allCards[group].Count : 9;
                float stride = tableWidth / (rowSize - 1);
                float z = group == 1 ? extraTop : sideTop;
                for (int cardIndex = 0; cardIndex < allCards[group].Count; ++cardIndex)
                {
                    allCards[group][cardIndex].MoveTo(new Vector3(mainLeft + stride * cardIndex, 0, z));
                }
                SideCount.text = sideCards.Count.ToString();
                ExtraCount.text = extraCards.Count.ToString();
            }
        }

        private void DeleteCurrentDeck()
        {
            int idx = dpdDecks.value;
            if (File.Exists(deckPath))
            {
                File.Delete(deckPath);
                deckModified = false;
                dpdDecks.options.Remove(dpdDecks.options[idx]);
                if (idx >= dpdDecks.options.Count)
                {
                    dpdDecks.value = dpdDecks.options.Count - 1;//event triggered
                }
                else
                {
                    dpdDecks.value = idx;
                    SwitchToDeck(Config.DeckPath + dpdDecks.options[idx].text + ".ydk");
                    dpdDecks.RefreshShownValue();
                }
            }
        }

        #region events
        private void OnDeckChanged(int value)
        {
            string newPath = Config.DeckPath + dpdDecks.options[value].text + ".ydk";
            if (deckPath.Equals(newPath))
            {
                return;
            }
            if (deckModified)
            {
                MainGame.Instance.ConfirmWindow.SelectYesNo(SysString(1356),
                    () => SwitchToDeck(newPath),
                    () =>
                    {
                        string origin = Path.GetFileNameWithoutExtension(deckPath);
                        dpdDecks.value = dpdDecks.options.FindIndex((op) => op.text.Equals(origin));
                    });
            }
            else
            {
                SwitchToDeck(newPath);
            }
        }

        private void OnFListChanged(int flist)
        {
            CurrentFList = DeckService.ForbiddenLists[flist];
            explorer.OnFListChanged();
            foreach (var buildCards in allCards)
            {
                foreach (var card in buildCards)
                {
                    card.UpdateIcon(CurrentFList.Query(card.Data));
                }
            }
        }

        private void OnClear()
        {
            deckModified = true;
            ClearDeck();
        }

        private void OnExit()
        {
            if (deckModified)
            {
                MainGame.Instance.ConfirmWindow.SelectYesNo(SysString(1356), Hide, null);
            }
            else
            {
                Config.Set("LastDeck", Path.GetFileNameWithoutExtension(deckPath));
                Hide();
            }
        }

        private void OnShuffle()
        {
            deckModified = true;
            System.Random random = new System.Random();
            for (int i = 0; i < 3; ++i)
            {
                for(int j=allCards[i].Count-1;j>=0;--j)
                {
                    allCards[i].Swap(j, random.Next() % (j + 1));
                }
                SpreadCard(i);
            }
        }

        private void OnSort()
        {
            deckModified = true;
            for (int i = 0; i < 3; ++i)
            {
                allCards[i].Sort((left, right) =>
                {
                    return CardData.CompareByStars(left.Data, right.Data);
                });
                SpreadCard(i);
            }
        }

        private void OnSave()
        {
            DeckService.CurrentDeck.Clear();
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < allCards[i].Count; ++j)
                {
                    DeckService.CurrentDeck.AllCards[i].Add(allCards[i][j].Data.Code);
                }
            }
            if (DeckService.Save(deckPath))
            {
                deckModified = false;
                MainGame.Instance.HintBox.ShowHint(SysString(1335), 1.2f);
                Config.Set("LastDeck", Path.GetFileNameWithoutExtension(deckPath));
            }
            else
            {
                MainGame.Instance.HintBox.ShowHint(TzlString(7), 1.4f);
            }
        }

        private void OnSaveAs()
        {
            void onOk(string input)
            {
                deckPath = Config.DeckPath + input + ".ydk";
                DeckService.CurrentDeck.Clear();
                for (int i = 0; i < 3; ++i)
                {
                    for (int j = 0; j < allCards[i].Count; ++j)
                    {
                        DeckService.CurrentDeck.AllCards[i].Add(allCards[i][j].Data.Code);
                    }
                }
                if (DeckService.Save(deckPath))
                {
                    deckModified = false;
                    int i = dpdDecks.options.FindIndex((s) => s.text.Equals(input));
                    if (i > 0)
                    {
                        dpdDecks.value = i;
                    }
                    else
                    {
                        dpdDecks.options.Insert(dpdDecks.value + 1, new TMP_Dropdown.OptionData(input));
                        dpdDecks.value++;
                    }
                    dpdDecks.RefreshShownValue();
                    Config.Set("LastDeck", input);
                }
                else { MainGame.Instance.HintBox.ShowHint("save failed", 1f); }
            }
            MainGame.Instance.InputBox.Show(TzlString(8), Path.GetFileNameWithoutExtension(deckPath), onOk, null);
        }

        private void OnRename()
        {
            void onOk(string input)
            {
                string renamedPath = Config.DeckPath + input + ".ydk";
                File.Move(deckPath, renamedPath);
                deckPath = renamedPath;
                dpdDecks.options[dpdDecks.value].text = input;
                dpdDecks.RefreshShownValue();
            }
            MainGame.Instance.InputBox.Show(TzlString(8), Path.GetFileNameWithoutExtension(deckPath), onOk, null);
        }

        private void OnDelete()
        {
            if (File.Exists(deckPath))
            {
                if (Config.GetBool("ConfirmDeleteDeck", true))
                {
                    MainGame.Instance.ConfirmWindow.SelectYesNo(SysString(1337), DeleteCurrentDeck, null);
                }
                else
                {
                    DeleteCurrentDeck();
                }
            }
        }

        private void OnFinishSide()
        {
            DeckService.CurrentDeck.Clear();
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < allCards[i].Count; ++j)
                {
                    DeckService.CurrentDeck.AllCards[i].Add(allCards[i][j].Data.Code);
                }
            }
            if (DeckService.CurrentDeck.Main.Count != preMainCount || DeckService.CurrentDeck.Extra.Count != preExtraCount)
            {
                MainGame.Instance.HintBox.ShowHint(SysString(1410), 2f);
            }
            else
            {
                NetworkService.Instance.Send(DeckService.CurrentDeck.ToArray());
                deckModified = false;
            }
        }

        #endregion
    }
}
