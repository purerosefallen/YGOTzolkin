using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YGOTzolkin.Elements;
using YGOTzolkin.Service;
using YGOTzolkin.Utility;
using YGOTzolkin.YGOModel;
using static YGOTzolkin.Service.DataService;

namespace YGOTzolkin.UI
{
    class CardExplorer : LazyWindow
    {
        class ExtendedOptionData : Dropdown.OptionData
        {
            public uint value;
            public ExtendedOptionData(string text, uint value) : base(text)
            {
                this.value = value;
            }
            public ExtendedOptionData() : base() { }
        }

        private int pageNumber;
        private int pageCount;
        private int itemCountPerPage;
        private float dragPoint;
        private Vector3 center;
        private Vector3 left;
        private Vector3 right;

        private CardData searchTarget;
        private List<CardData> pageContent;

        private PagePanel currentPage;
        private PagePanel previousPage;
        private PagePanel nextPage;

        private List<Comparison<CardData>> cardComparisons;

        private Canvas detailCanvas;
        private Canvas monsterCanvas;
        private Canvas stCanvas;

        private TMP_InputField iptKeyword;
        private Toggle tglDetail;
        private Dropdown dpdSortType;
        private TextMeshProUGUI txtResultCount;
        //detailed
        private Toggle tglEffect;
        private Dropdown dpdFilters;
        //monster
        private Toggle tglMonster;
        private Dropdown dpdMTypes;
        private Dropdown dpdRace;
        private List<Toggle> attributes;
        private List<Toggle> linkMarkers;
        private TMP_InputField iptStars;
        private TMP_InputField IptScale;
        private TMP_InputField iptAttack;
        private TMP_InputField iptDefence;
        //spell trap
        private Toggle tglSpellTrap;
        private Toggle tglSpellNormal;
        private Toggle tglSpellQuickPlay;
        private Toggle tglSpellEquip;
        private Toggle tglSpellRitual;
        private Toggle tglSpellField;
        private Toggle tglSpellContinuous;
        private Toggle tglTrapNormal;
        private Toggle tglTrapContinuous;
        private Toggle tglTrapCounter;
        //effect
        private Canvas effectCanvas;
        private List<Toggle> effectTgls;
        public CardExplorer()
        {

        }

        protected override void LazyInitialize()
        {
            MainCanvas = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/CardExplorer") as GameObject).GetComponent<Canvas>();
            TryGetControl(out detailCanvas, "detailCanvas");
            TryGetControl(out monsterCanvas, "monsterCanvas");
            TryGetControl(out stCanvas, nameof(stCanvas));
            stCanvas.enabled = false;
            detailCanvas.enabled = false;
            monsterCanvas.enabled = false;

            TryGetControl(out dpdSortType, "DpdSortType");
            List<ExtendedOptionData> sortOptions = new List<ExtendedOptionData>
            {
                new ExtendedOptionData(SysString(1370),0),
                new ExtendedOptionData(SysString(1371),1),
                new ExtendedOptionData(SysString(1372),2),
                new ExtendedOptionData(SysString(1373),3),
            };
            cardComparisons = new List<Comparison<CardData>>
            {
                CardData.CompareByStars,
                CardData.CompareByAttack,
                CardData.CompareByDefense,
                CardData.CompareByName,
            };
            dpdSortType.options.AddRange(sortOptions);
            dpdSortType.onValueChanged.AddListener(OnSortTypeChanged);

            tglDetail = GetControl<Toggle>("TglDetail");
            tglDetail.onValueChanged.AddListener(OnDetailed);
            tglDetail.SetToggleName(TzlString(11));

            iptKeyword = GetControl<TMP_InputField>("IptKeyword");
            iptKeyword.onValueChanged.AddListener(OnSearchEvent);
            iptKeyword.onSubmit.AddListener(OnSearchEvent);
            txtResultCount = GetControl<TextMeshProUGUI>("TxtResultCount");

            var scroll = GetControl<RectTransform>("ScrollBody");
            Tools.BindEvent(scroll.gameObject, EventTriggerType.BeginDrag, OnBeginDrag);
            Tools.BindEvent(scroll.gameObject, EventTriggerType.EndDrag, OnEndDrag);
            Tools.BindEvent(scroll.gameObject, EventTriggerType.Scroll, OnScroll);
            TryGetControl(out currentPage, "PnlCurrent");
            center = currentPage.MainRect.anchoredPosition3D;
            TryGetControl(out nextPage, "PnlNext");
            right = nextPage.MainRect.anchoredPosition3D;
            TryGetControl(out previousPage, "PnlPrevious");
            left = previousPage.MainRect.anchoredPosition3D;
            itemCountPerPage = 6;
            currentPage.Initialize(itemCountPerPage, 0);
            nextPage.Initialize(itemCountPerPage, 1);
            previousPage.Initialize(itemCountPerPage, 2);
            searchTarget = new CardData();
            //detail
            tglEffect = GetControl<Toggle>("tglEffect");
            tglEffect.onValueChanged.AddListener(OnEffect);
            tglEffect.SetToggleName(SysString(1326));
            TryGetControl(out effectCanvas, nameof(effectCanvas));
            effectCanvas.enabled = false;
            effectTgls = new List<Toggle>(32);
            var pnleffects = GetControl<RectTransform>("pnleffects");
            for (uint i = 0; i < 32; ++i)
            {
                Toggle toggle = pnleffects.GetChild((int)i).GetComponent<Toggle>();
                toggle.SetToggleName(SysString(1100 + i));
                toggle.isOn = false;
                toggle.onValueChanged.AddListener(OnSearchEvent);
                effectTgls.Add(toggle);
            }

            var reset = GetControl<Button>("BtnReset");
            reset.SetButtonName(SysString(1309));
            reset.onClick.AddListener(OnReset);
            TryGetControl(out dpdFilters, "DpdFilters");
            dpdFilters.options.Add(new Dropdown.OptionData(TzlString(14)));
            for (uint i = 1316; i <= 1318; ++i) //flist
            {
                dpdFilters.options.Add(new Dropdown.OptionData(SysString(i)));
            }
            for (uint i = 1240; i <= 1243; ++i)//ocg&tcg
            {
                dpdFilters.options.Add(new Dropdown.OptionData(SysString(i)));
            }
            dpdFilters.onValueChanged.AddListener(OnSearchEvent);
            //monster
            tglMonster = GetControl<Toggle>("TglMonster");
            tglMonster.SetToggleName(SysString(1312));
            tglMonster.onValueChanged.AddListener(OnMonster);

            TryGetControl(out dpdMTypes, "DpdMType");
            dpdMTypes.options.Add(new ExtendedOptionData(TzlString(12), 0));
            int allMonsterType = (int)CardType.Normal + (int)CardType.Flip + (int)CardType.Toon
                + (int)CardType.Effect + (int)CardType.Fusion + (int)CardType.Ritual
                + (int)CardType.Spirit + (int)CardType.Union + (int)CardType.Dual
                + (int)CardType.Tuner + (int)CardType.Synchro + (int)CardType.Xyz
                + (int)CardType.Pendulum + (int)CardType.SpecialSummon + (int)CardType.Link;
            for (uint i = 1050, filter = 1; filter != 0x8000000; filter <<= 1, ++i)
            {
                if ((filter & allMonsterType) > 0)
                {
                    dpdMTypes.options.Add(new ExtendedOptionData(SysString(i), filter));
                }
            }
            uint val = (int)CardType.Synchro + (int)CardType.Tuner;
            dpdMTypes.options.Add(new ExtendedOptionData(FormatType(val), val));
            val = (int)CardType.Normal + (int)CardType.Tuner;
            dpdMTypes.options.Add(new ExtendedOptionData(FormatType(val), val));
            val = (int)CardType.Normal + (int)CardType.Pendulum;
            dpdMTypes.options.Add(new ExtendedOptionData(FormatType(val), val));
            val = (int)CardType.Xyz + (int)CardType.Pendulum;
            dpdMTypes.options.Add(new ExtendedOptionData(FormatType(val), val));
            val = (int)CardType.Fusion + (int)CardType.Pendulum;
            dpdMTypes.options.Add(new ExtendedOptionData(FormatType(val), val));
            val = (int)CardType.Synchro + (int)CardType.Pendulum;
            dpdMTypes.options.Add(new ExtendedOptionData(FormatType(val), val));
            dpdMTypes.RefreshShownValue();
            dpdMTypes.onValueChanged.AddListener(OnSearchEvent);

            TryGetControl(out dpdRace, "DpdRace");
            dpdRace.options.Add(new ExtendedOptionData(TzlString(13), 0));
            for (uint i = 1020, filter = 1; filter != 0x2000000; filter <<= 1, ++i)
            {
                dpdRace.options.Add(new ExtendedOptionData(SysString(i), filter));
            }
            dpdRace.RefreshShownValue();
            dpdRace.onValueChanged.AddListener(OnSearchEvent);

            linkMarkers = new List<Toggle>(8);
            foreach (LinkMarker marker in Enum.GetValues(typeof(LinkMarker)))
            {
                var tgl = GetControl<Toggle>("tgl" + marker.ToString());
                tgl.onValueChanged.AddListener(OnSearchEvent);
                linkMarkers.Add(tgl);
            }
            attributes = new List<Toggle>(7);
            foreach (CardAttribute attr in Enum.GetValues(typeof(CardAttribute)))
            {
                var tgl = GetControl<Toggle>("tgl" + attr.ToString());
                tgl.onValueChanged.AddListener(OnSearchEvent);
                attributes.Add(tgl);
            }

            TryGetControl(out iptAttack, nameof(iptAttack));
            TryGetControl(out iptDefence, nameof(iptDefence));
            TryGetControl(out IptScale, nameof(IptScale));
            TryGetControl(out iptStars, nameof(iptStars));
            iptStars.onSubmit.AddListener(OnSearchEvent);
            iptDefence.onSubmit.AddListener(OnSearchEvent);
            IptScale.onSubmit.AddListener(OnSearchEvent);
            iptAttack.onSubmit.AddListener(OnSearchEvent);
            iptStars.placeholder.GetComponent<TextMeshProUGUI>().text = SysString(1324);
            IptScale.placeholder.GetComponent<TextMeshProUGUI>().text = SysString(1336);
            iptAttack.placeholder.GetComponent<TextMeshProUGUI>().text = SysString(1322);
            iptDefence.placeholder.GetComponent<TextMeshProUGUI>().text = SysString(1323);

            TryGetControl(out tglSpellTrap, nameof(tglSpellTrap));
            tglSpellTrap.SetToggleName(SysString(1051) + SysString(1052));
            tglSpellTrap.onValueChanged.AddListener(OnSpellTrap);
            TryGetControl(out TextMeshProUGUI spell, "txtspell");
            spell.text = SysString(1051);
            TryGetControl(out TextMeshProUGUI trap, "txttrap");
            trap.text = SysString(1052);

            TryGetControl(out tglSpellNormal, nameof(tglSpellNormal));
            tglSpellNormal.SetToggleName(SysString(1054));
            tglSpellNormal.onValueChanged.AddListener(OnSearchEvent);

            TryGetControl(out tglSpellQuickPlay, nameof(tglSpellQuickPlay));
            tglSpellQuickPlay.SetToggleName(SysString(1066));
            tglSpellQuickPlay.onValueChanged.AddListener(OnSearchEvent);

            TryGetControl(out tglSpellEquip, nameof(tglSpellEquip));
            tglSpellEquip.SetToggleName(SysString(1068));
            tglSpellEquip.onValueChanged.AddListener(OnSearchEvent);

            TryGetControl(out tglSpellRitual, nameof(tglSpellRitual));
            tglSpellRitual.SetToggleName(SysString(1057));
            tglSpellRitual.onValueChanged.AddListener(OnSearchEvent);

            TryGetControl(out tglSpellField, nameof(tglSpellField));
            tglSpellField.SetToggleName(SysString(1069));
            tglSpellField.onValueChanged.AddListener(OnSearchEvent);

            TryGetControl(out tglSpellContinuous, nameof(tglSpellContinuous));
            tglSpellContinuous.SetToggleName(SysString(1067));
            tglSpellContinuous.onValueChanged.AddListener(OnSearchEvent);

            TryGetControl(out tglTrapNormal, nameof(tglTrapNormal));
            tglTrapNormal.SetToggleName(SysString(1054));
            tglTrapNormal.onValueChanged.AddListener(OnSearchEvent);

            TryGetControl(out tglTrapContinuous, nameof(tglTrapContinuous));
            tglTrapContinuous.SetToggleName(SysString(1067));
            tglTrapContinuous.onValueChanged.AddListener(OnSearchEvent);

            TryGetControl(out tglTrapCounter, nameof(tglTrapCounter));
            tglTrapCounter.SetToggleName(SysString(1070));
            tglTrapCounter.onValueChanged.AddListener(OnSearchEvent);
            Hide();
        }

        internal void FocusInput()
        {
            iptKeyword.ActivateInputField();
        }

        private void PageForward()
        {
            if (pageNumber == pageCount - 1 || pageCount == 0)
            {
                return;
            }
            currentPage.MoveTo(left);
            nextPage.MoveTo(center);
            previousPage.MainRect.anchoredPosition3D = right;
            previousPage.UpdatePage(pageContent, (pageNumber + 2) * itemCountPerPage);
            pageNumber++;
            txtResultCount.text = string.Format("{0}/{1} {2}", pageNumber + 1, pageCount, pageContent.Count);
            var temp = currentPage;
            currentPage = nextPage;
            nextPage = previousPage;
            previousPage = temp;
        }

        private void PageBack()
        {
            if (pageNumber == 0)
            {
                return;
            }
            currentPage.MoveTo(right);
            previousPage.MoveTo(center);
            nextPage.MainRect.anchoredPosition3D = left;
            nextPage.UpdatePage(pageContent, (pageNumber - 2) * itemCountPerPage);
            pageNumber--;
            txtResultCount.text = string.Format("{0}/{1} {2}", pageNumber + 1, pageCount, pageContent.Count);
            var temp = currentPage;
            currentPage = previousPage;
            previousPage = nextPage;
            nextPage = temp;
        }

        private void UpdateContent()
        {
            if (pageContent == null)
            {
                pageCount = 0;
                pageNumber = 0;
                return;
            }
            pageNumber = 0;
            pageCount = pageContent.Count / itemCountPerPage + (pageContent.Count % itemCountPerPage == 0 ? 0 : 1);
            if (pageContent.Count == 0)
            {
                txtResultCount.text = ("0/0 0");
            }
            else
            {
                txtResultCount.text = string.Format("{0}/{1} {2}", pageNumber + 1, pageCount, pageContent.Count);
            }
            currentPage.UpdatePage(pageContent, 0);
            nextPage.UpdatePage(pageContent, itemCountPerPage);
        }

        private void Search()
        {
            searchTarget.Reset();
            searchTarget.Name = iptKeyword.text;
            if (!tglDetail.isOn)
            {
                pageContent = DataService.Search(searchTarget, MainGame.Instance.DeckBuilder.CurrentFList, 3);
                pageContent.Sort(cardComparisons[dpdSortType.value]);
                UpdateContent();
                return;
            }
            int qualification = 3;
            switch (dpdFilters.value)
            {
                case 1:
                case 2:
                case 3:
                    qualification = dpdFilters.value - 1;
                    break;
                case 4:
                case 5:
                case 6:
                case 7:
                    searchTarget.OTExclusive = (uint)dpdFilters.value - 3;
                    break;
                default:
                    break;
            }
            if (tglEffect.isOn)
            {
                uint filter = 0x1;
                for (int i = 0; i < 32; ++i, filter <<= 1)
                {
                    if (effectTgls[i].isOn)
                    {
                        searchTarget.Category |= filter;
                    }
                }
            }
            if (tglMonster.isOn)
            {
                searchTarget.Type += 0x1;
                ExtendedOptionData data = dpdMTypes.options[dpdMTypes.value] as ExtendedOptionData;
                searchTarget.Type += data.value;
                data = dpdRace.options[dpdRace.value] as ExtendedOptionData;
                searchTarget.Race = data.value;
                uint filter = 1;
                int i = 0;
                for (; filter != 0x80; filter <<= 1, ++i)
                {
                    if (attributes[i].isOn)
                    {
                        searchTarget.Attribute += filter;
                    }
                }
                if (int.TryParse(iptAttack.text, out i))
                {
                    searchTarget.Attack = i;
                }
                if (int.TryParse(iptDefence.text, out i))
                {
                    searchTarget.Defense = i;
                }
                if (int.TryParse(iptStars.text, out i))
                {
                    searchTarget.LScale = (uint)i;
                }
                if (int.TryParse(iptStars.text, out i))
                {
                    searchTarget.Level = (uint)i;
                }
                i = 0;
                foreach (LinkMarker marker in Enum.GetValues(typeof(LinkMarker)))
                {
                    if (linkMarkers[i].isOn)
                    {
                        searchTarget.LinkMarker += (uint)marker;
                    }
                    i++;
                }
                pageContent = DataService.Search(searchTarget, MainGame.Instance.DeckBuilder.CurrentFList, qualification);
                pageContent.Sort(cardComparisons[dpdSortType.value]);
                UpdateContent();
            }
            else if (tglSpellTrap.isOn)
            {
                //attribute for trap type
                if (!tglSpellNormal.isOn && !tglSpellRitual.isOn && !tglSpellQuickPlay.isOn && !tglSpellContinuous.isOn
                    && !tglSpellEquip.isOn && !tglSpellField.isOn && !tglTrapNormal.isOn && !tglTrapContinuous.isOn && !tglTrapCounter.isOn)
                {
                    searchTarget.Type = 0x2;
                    pageContent = DataService.Search(searchTarget, MainGame.Instance.DeckBuilder.CurrentFList, qualification);
                    searchTarget.Attribute = 0x4;
                    searchTarget.Type = 0;
                    pageContent.AddRange(DataService.Search(searchTarget, MainGame.Instance.DeckBuilder.CurrentFList, qualification));
                    pageContent.Sort(cardComparisons[dpdSortType.value]);
                    UpdateContent();
                }
                else
                {
                    if (tglSpellNormal.isOn)
                    {
                        searchTarget.Type |= 0x2;
                        searchTarget.Type |= (uint)CardType.Normal;
                    }
                    if (tglSpellRitual.isOn)
                    {
                        searchTarget.Type |= (uint)CardType.Ritual;
                        searchTarget.Type |= 0x2;
                    }
                    if (tglSpellQuickPlay.isOn)
                    {
                        searchTarget.Type |= (uint)CardType.QuickPlay;
                        searchTarget.Type |= 0x2;
                    }
                    if (tglSpellContinuous.isOn)
                    {
                        searchTarget.Type |= (uint)CardType.Continuous;
                        searchTarget.Type |= 0x2;
                    }
                    if (tglSpellEquip.isOn)
                    {
                        searchTarget.Type |= (uint)CardType.Equip;
                        searchTarget.Type |= 0x2;
                    }
                    if (tglSpellField.isOn)
                    {
                        searchTarget.Type |= (int)CardType.Field;
                        searchTarget.Type |= 0x2;
                    }
                    bool spellSearched = false;
                    if (searchTarget.Type != 0)
                    {
                        pageContent = DataService.Search(searchTarget, MainGame.Instance.DeckBuilder.CurrentFList, qualification);
                        spellSearched = true;
                    }
                    searchTarget.Type = 0;
                    if (tglTrapNormal.isOn)
                    {
                        searchTarget.Attribute |= 0x4;
                        searchTarget.Attribute |= (uint)CardType.Normal;
                    }
                    if (tglTrapContinuous.isOn)
                    {
                        searchTarget.Attribute |= (uint)CardType.Continuous;
                        searchTarget.Attribute |= 0x4;
                    }
                    if (tglTrapCounter.isOn)
                    {
                        searchTarget.Attribute |= (uint)CardType.Counter;
                        searchTarget.Attribute |= 0x4;
                    }
                    if (searchTarget.Attribute != 0)
                    {
                        if (spellSearched)
                        {
                            pageContent.AddRange(DataService.Search(searchTarget, MainGame.Instance.DeckBuilder.CurrentFList, qualification));
                        }
                        else
                        {
                            pageContent = DataService.Search(searchTarget, MainGame.Instance.DeckBuilder.CurrentFList, qualification);
                        }
                    }
                    pageContent.Sort(cardComparisons[dpdSortType.value]);
                    UpdateContent();
                }
            }
            else
            {
                pageContent = DataService.Search(searchTarget, MainGame.Instance.DeckBuilder.CurrentFList, qualification);
                pageContent.Sort(cardComparisons[dpdSortType.value]);
                UpdateContent();
            }
        }

        #region event

        internal void OnFListChanged()
        {
            if (tglDetail.isOn && dpdFilters.value > 0 && dpdFilters.value < 4)
            {
                Search();
            }
        }

        private void OnMonster(bool value)
        {
            monsterCanvas.enabled = value;
            if (value)
            {
                tglSpellTrap.isOn = false;
            }
            Search();
        }

        private void OnSpellTrap(bool value)
        {
            stCanvas.enabled = value;
            if (value)
            {
                tglMonster.isOn = false;
            }
            Search();
        }

        private void OnDetailed(bool value)
        {
            detailCanvas.enabled = value;
            Search();
        }

        private void OnSortTypeChanged(int value)
        {
            if (pageContent == null)
            {
                return;
            }
            pageContent.Sort(cardComparisons[dpdSortType.value]);
            UpdateContent();
        }

        private void OnEffect(bool value)
        {
            effectCanvas.enabled = value;
            if (!value)
            {
                for (int i = 0; i < 32; ++i)
                {
                    effectTgls[i].isOn = false;
                }
            }
            Search();
        }

        private void OnReset()
        {
            iptKeyword.text = "";
            dpdFilters.value = 0;
            tglMonster.isOn = false;
            tglSpellTrap.isOn = false;
            tglEffect.isOn = false;//event triggered
        }

        private void OnScroll(BaseEventData data)
        {
            if (pageContent == null || pageContent.Count == 0)
            {
                return;
            }
            PointerEventData scrollData = data as PointerEventData;
            Vector2 delta = scrollData.scrollDelta;
            if (delta.y < 0)
            {
                PageForward();
            }
            else
            {
                PageBack();
            }
        }

        private void OnBeginDrag(BaseEventData data)
        {
            PointerEventData pointer = data as PointerEventData;
            dragPoint = pointer.currentInputModule.input.mousePosition.x;
        }

        private void OnEndDrag(BaseEventData data)
        {
            PointerEventData pointer = data as PointerEventData;
            if (dragPoint > pointer.currentInputModule.input.mousePosition.x)
            {
                PageForward();
            }
            else
            {
                PageBack();
            }
        }

        private void OnSearchEvent<T>(T param)
        {
            Search();
        }


        #endregion
    }
}
