using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YGOTzolkin.Elements;
using YGOTzolkin.Service;
using YGOTzolkin.YGOModel;
using YGOTzolkin.Utility;
using System;

namespace YGOTzolkin
{
    class Field
    {
        private readonly ObjectPool<ClientCard> cardPool;
        private readonly ObjectPool<Chain> chainPool;
        private readonly List<ClientCard> activeCards;
        private readonly Dictionary<int, ClientCard> sequencePointer;

        private readonly Transform inertiaTransform;
        private readonly Dictionary<Transform, ClientCard> spreadingTranforms;
        private Vector3 velocity;
        private float friction;
        private float leftEnd;
        private int orginalChildCount;
        private bool needSpread;
        private readonly List<int> sumAssist;
        private GameObject fieldObject;

        private float startX = 0;
        private bool dragging = false;
        private float preXpos = 0;
        private float xDiff = 0;
        internal Transform HitTransform { get; private set; }

        internal ClientCard Attacker { get; set; }
        internal List<Chain> CurrentChains { get; private set; }
        internal List<ClientCard>[] HandCards { get; private set; }
        internal MapList<ClientCard> InteractiveCards { get; private set; }

        internal bool IsConfirming { get; private set; }

        internal List<ClientCard> SelectableCards { get; private set; }
        internal List<ClientCard> SelectedCards { get; private set; }
        internal int SelectMin { get; set; }
        internal int SelectMax { get; set; }
        internal int SelectSumVal { get; set; }
        internal int SelectSumFixedCount { get; set; }
        internal bool Cancelabel { get; set; }
        internal bool Finishable { get; set; }
        internal bool Overflowable { get; set; }
        internal uint CounterToBeSelected { get; set; }

        public Field()
        {
            cardPool = new ObjectPool<ClientCard>();
            chainPool = new ObjectPool<Chain>();
            activeCards = new List<ClientCard>();
            sequencePointer = new Dictionary<int, ClientCard>();

            HandCards = new List<ClientCard>[]
            {
                new List<ClientCard>(),
                new List<ClientCard>(),
            };
            CurrentChains = new List<Chain>();
            InteractiveCards = new MapList<ClientCard>();

            inertiaTransform = new GameObject().transform;
            inertiaTransform.name = "InertiaSlider";
            inertiaTransform.position = Vector3.zero;
            orginalChildCount = 0;
            spreadingTranforms = new Dictionary<Transform, ClientCard>();
            SelectableCards = new List<ClientCard>();
            SelectedCards = new List<ClientCard>();
            sumAssist = new List<int>(256);
            fieldObject = null;
        }

        internal void Update()
        {
            xDiff = Input.mousePosition.x - preXpos;
            preXpos = Input.mousePosition.x;

            Ray ray = MainGame.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                HitTransform = hit.transform;
            }
            else
            {
                HitTransform = null;
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (dragging)
                {
                    velocity.x = xDiff / 30;
                    friction = 0;
                }
                dragging = false;
            }
            if (dragging)
            {
                friction = 1;
                velocity.x = xDiff / 10;
            }
            if (Input.GetMouseButtonDown(0))
            {
                startX = Input.mousePosition.x;
                if (!dragging && (HitTransform == null || !hit.transform.CompareTag("Card") && !hit.transform.CompareTag("CommandButton")))
                {
                    EndQuery();
                }
            }
            if (Input.GetMouseButton(0))
            {
                if (!dragging && Mathf.Abs(Input.mousePosition.x - startX) > 5)
                {
                    dragging = true;
                }
            }
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                velocity.x = 1.5f;
                friction = 0;
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                velocity.x = -1.5f;
                friction = 0;
            }
            //inertia movement
            inertiaTransform.position += velocity;
            float x = inertiaTransform.position.x;
            if (x < leftEnd || x > 0)
            {
                x = x < leftEnd ? leftEnd - x : -x;
                velocity.x = x * 0.3f;
            }
            else
            {
                velocity = Vector3.Lerp(velocity, Vector3.zero, friction);
                friction += Time.smoothDeltaTime;
            }
            //card monitoring
            if (needSpread || inertiaTransform.childCount != orginalChildCount)
            {
                orginalChildCount = inertiaTransform.childCount;
                needSpread = false;
                float interval = 6.2f;
                float left = -21.7f;
                if (inertiaTransform.childCount <= 8)
                {
                    left = interval * (inertiaTransform.childCount - 1) * -0.5f;
                    leftEnd = 0;
                }
                else
                {
                    leftEnd = (inertiaTransform.childCount - 8) * interval * -1;
                }
                var center = Vector3.zero;
                for (int i = 0; i < inertiaTransform.childCount; ++i)
                {
                    center.x = left + i * interval;
                    var card = spreadingTranforms[inertiaTransform.GetChild(i)];
                    card.QueryingPosition = center;
                    card.AddAnimation(card.MoveQuery(), 0);
                }
            }
        }

        internal void ResetSelection()
        {
            DetachAll();
            foreach (var c in SelectableCards)
            {
                c.Selectable = false;
                c.Selected = false;
                c.SelectionParam = 0;
            }
            SelectableCards.Clear();
            SelectedCards.ForEach((c) => c.Selected = false);
            SelectedCards.Clear();
            Finishable = false;
            Cancelabel = false;
        }

        private void DetachAll()
        {
            for (int i = inertiaTransform.childCount - 1; i >= 0; --i)
            {
                var card = spreadingTranforms[inertiaTransform.GetChild(i).transform];
                card.Detach();
                card.AddAnimation(card.MoveRealSmoothly(0.15f), 0);
            }
            spreadingTranforms.Clear();
        }

        internal void SpreadConfirm(IEnumerable<ClientCard> cards)
        {
            EndQuery();
            IsConfirming = true;
            Spread(cards);
        }

        internal void Query(List<ClientCard> cards)
        {
            if (SelectableCards.Count > 0)
            {
                return;
            }
            EndQuery();
            cards.Sort(ClientCard.QueryCompare);
            Spread(cards, true);
        }

        internal void EndQuery()
        {
            if (SelectableCards.Count == 0 && spreadingTranforms.Count > 0)
            {
                DetachAll();
                IsConfirming = false;
            }
        }

        internal void SpreadSelection()
        {
            Spread(SelectableCards);
        }

        internal void SpreadAvailable()
        {
            if (spreadingTranforms.Count == InteractiveCards.Count)
            {
                Debug.Log("-->count equals");
                int a = 0;
                foreach (var c in InteractiveCards)
                {
                    if (spreadingTranforms.ContainsKey(c.Transform))
                    {
                        a++;
                    }
                }
                if (a == InteractiveCards.Count)
                {
                    return;
                }
                Debug.Log("-->contain not equals");

            }
            EndQuery();
            InteractiveCards.Sort(ClientCard.ActivateCompare);
            Spread(InteractiveCards);
        }

        private void Spread(IEnumerable<ClientCard> cards, bool querying = false)
        {
            Vector3 center;
            if (querying && GameInfo.Instance.PlayerType != PlayerType.Observer)
            {
                center = Vector3.Lerp(new Vector3(0, 0, -25), MainGame.Instance.MainCamera.transform.position, 0.15f);
            }
            else
            {
                center = Vector3.Lerp(Vector3.zero, MainGame.Instance.MainCamera.transform.position, 0.35f);
            }
            inertiaTransform.position = center;
            foreach (var card in cards)
            {
                card.Transform.SetParent(inertiaTransform);
                spreadingTranforms.Add(card.Transform, card);
                card.GroupText.text = DataService.FormatLocation(card.Location, card.Sequence);
                card.GroupText.transform.localScale = Vector3.one;
            }
            needSpread = true;
        }

        internal void OnCardSelected(ClientCard card)
        {
            Debug.Log(string.Format("-->{0} is selected by clicking", DataService.GetName(card.Data.Code)));
            switch (GameInfo.Instance.CurrentMessage)
            {
                case GameMessage.SelectCounter:
                    card.SelectionParam--;
                    if ((card.SelectionParam & 0xffff) == 0)
                    {
                        card.Selectable = false;
                    }
                    SelectMin--;
                    if (SelectMin == 0)
                    {
                        byte[] response = new byte[SelectableCards.Count * 2];
                        BinaryWriter writer = new BinaryWriter(new MemoryStream(response));
                        for (int i = 0; i < SelectableCards.Count; ++i)
                        {
                            writer.Write((ushort)((SelectableCards[i].SelectionParam >> 16) - (SelectableCards[i].SelectionParam & 0xffff)));
                        }
                        ResetSelection();
                        MainGame.Instance.SendCToSResponse(response);
                    }
                    else
                    {
                        MainGame.Instance.DuelWindow.ShowSelectHint(
                            string.Format(DataService.SysString(204), SelectMin, DataService.CounterName(CounterToBeSelected)));
                    }
                    break;
                case GameMessage.SelectCard:
                    if (card.Selected)
                    {
                        SelectedCards.Remove(card);
                        card.Selected = false;
                    }
                    else
                    {
                        SelectedCards.Add(card);
                        card.Selected = true;
                    }
                    if ((SelectedCards.Count >= SelectMax) || (SelectedCards.Count == SelectableCards.Count))
                    {
                        SendSelectResponse();
                    }
                    else if (SelectedCards.Count >= SelectMin)
                    {
                        Finishable = true;
                        MainGame.Instance.DuelWindow.SetCancelOrFinish(2);
                    }
                    break;
                case GameMessage.SelectTribute:
                    if (card.Selected)
                    {
                        SelectedCards.Remove(card);
                        card.Selected = false;
                    }
                    else
                    {
                        SelectedCards.Add(card);
                        card.Selected = true;
                    }
                    if (SelectedCards.Count == SelectMax || (SelectedCards.Count == SelectableCards.Count))
                    {
                        SendSelectResponse();
                        return;
                    }
                    int acc = 0;
                    foreach (var c in SelectedCards)
                    {
                        acc += c.SelectionParam;
                    }
                    if (acc > SelectMin)
                    {
                        MainGame.Instance.DuelWindow.SetCancelOrFinish(2);
                        Finishable = true;
                    }
                    break;
                case GameMessage.SelectSum:
                    if (card.Selected)
                    {
                        SelectedCards.Remove(card);
                        card.Selected = false;
                    }
                    else
                    {
                        SelectedCards.Add(card);
                        card.Selected = true;
                    }
                    UpdateSelectSum(spreadingTranforms.Count != 0);
                    break;
                case GameMessage.SelectUnSelectCard:
                    SelectedCards.Add(card);
                    SendSelectResponse();
                    break;
                case GameMessage.SortCard:
                    //todo impl
                    break;
                default: break;
            }
        }

        internal void UpdateSelectSum(bool spreading)
        {
            sumAssist.Clear();
            sumAssist.Add(0);
            int count, p1, p2, maxParam = -1, minParam = int.MaxValue, selectedCount = SelectedCards.Count - SelectSumFixedCount;
            HashSet<ClientCard> available = new HashSet<ClientCard>();
            var remains = new List<ClientCard>(SelectableCards);
            remains.RemoveAll((c) => c.Selected);
            for (int i = 0; i < SelectedCards.Count; ++i)
            {
                count = sumAssist.Count;
                p1 = SelectedCards[i].SelectionParam & 0xffff;
                p2 = SelectedCards[i].SelectionParam >> 16;
                int pmin = (p2 > 0 && p2 < p1) ? p2 : p1;
                minParam = minParam < pmin ? minParam : pmin;
                maxParam = maxParam > p1 ? maxParam : p1;
                maxParam = maxParam > p2 ? maxParam : p2;
                for (int j = 0; j < count; ++j)
                {
                    if (p2 > 0)
                    {
                        sumAssist.Add(sumAssist[j] + p2);
                    }
                    sumAssist[j] += p1;
                }
            }
            if (Overflowable)
            {
                sumAssist.Sort();
                int sumMax = sumAssist[sumAssist.Count - 1];
                int sumMin = sumAssist[0];
                if (sumMin >= SelectSumVal)
                {
                    SendSelectResponse();
                    return;
                }
                else if (sumMax >= SelectSumVal && sumMax - maxParam < SelectSumVal)
                {
                    Finishable = true;
                    MainGame.Instance.DuelWindow.SetCancelOrFinish(2);
                }
                int sum;
                for (int i = 0; i < remains.Count; ++i)
                {
                    remains.Swap(0, i);
                    p1 = remains[0].SelectionParam & 0xffff;
                    p2 = remains[0].SelectionParam >> 16;
                    sum = sumMin + p1;
                    minParam = minParam < p1 ? minParam : p1;
                    if (sum >= SelectSumVal)
                    {
                        if (sum - minParam < SelectSumVal)
                        {
                            available.Add(remains[0]);
                        }
                    }
                    else if (CheckMin(1, remains, SelectSumVal - sum, SelectSumVal - sum + minParam - 1))
                    {
                        available.Add(remains[0]);
                    }
                    if (p2 == 0)
                    {
                        continue;
                    }
                    sum = sumMin + p2;
                    minParam = minParam < p2 ? minParam : p2;
                    if (sum >= SelectSumVal)
                    {
                        if (sum - minParam < SelectSumVal)
                        {
                            available.Add(remains[0]);
                        }
                    }
                    else if (CheckMin(1, remains, SelectSumVal - sum, SelectSumVal - sum + minParam - 1))
                    {
                        available.Add(remains[0]);
                    }
                }
            }
            else
            {
                if (sumAssist.Contains(SelectSumVal) && selectedCount >= SelectMin && selectedCount <= SelectMax)
                {
                    SendSelectResponse();
                    return;
                }
                HashSet<int> noduplicate = new HashSet<int>(sumAssist);
                foreach (int val in noduplicate)
                {
                    int tarValue = SelectSumVal - val;
                    for (int i = 0; i < remains.Count; ++i)
                    {
                        if (available.Contains(remains[i]))
                        {
                            continue;
                        }
                        remains.Swap(0, i);
                        p1 = remains[0].SelectionParam & 0xffff;
                        p2 = remains[0].SelectionParam >> 16;
                        if (CheckSum(1, remains, tarValue - p1, selectedCount + 1)
                            || (p2 > 0 && CheckSum(1, remains, tarValue - p2, selectedCount + 1)))
                        {
                            available.Add(remains[0]);
                        }
                    }
                }
            }
            if (!Finishable && available.Count == 1)
            {
                SelectedCards.AddRange(available);
                SendSelectResponse();
                return;
            }
            if (spreading)
            {
                DetachAll();
                Spread(available);
            }
            else
            {
                SelectableCards.ForEach((c) => c.Selectable = false);
                foreach (var c in available)
                {
                    c.Selectable = true;
                }
            }
        }

        private bool CheckMin(int index, List<ClientCard> remains, int min, int max)
        {
            if (index == remains.Count)
            {
                return false;
            }
            int l1 = remains[index].SelectionParam & 0xffff;
            int l2 = remains[index].SelectionParam >> 16;
            int m = (l2 > 0 && l2 < l1) ? l2 : l1;
            if (m >= min && m <= max)
            {
                return true;
            }
            index++;
            return (min > m && CheckMin(index, remains, min - m, max - m))
                || CheckMin(index, remains, min, max);
        }

        private bool CheckSum(int index, List<ClientCard> remains, int acc, int count)
        {
            if (acc == 0)
            {
                return count >= SelectMin && count <= SelectMax;
            }
            if (acc < 0 || index == remains.Count)
            {
                return false;
            }
            int p = remains[index].SelectionParam;
            int p1 = p & 0xffff;
            int p2 = p >> 16;
            if ((p1 == acc || (p2 > 0 && p2 == acc)) && (count + 1 >= SelectMin) && (count + 1 <= SelectMax))
            {
                return true;
            }
            ++index;
            return (acc > p1 && CheckSum(index, remains, acc - p1, count + 1))
                || (p2 > 0 && acc > p2 && CheckSum(index, remains, acc - p2, count + 1))
                || CheckSum(index, remains, acc, count);
        }

        internal void SendSelectResponse()
        {
            byte[] response = new byte[SelectedCards.Count + 1];
            response[0] = (byte)SelectedCards.Count;
            for (int i = 0; i < SelectedCards.Count; ++i)
            {
                response[i + 1] = SelectedCards[i].SelectSequence;
            }
            MainGame.Instance.SendCToSResponse(response);
            ResetSelection();
            MainGame.Instance.DuelWindow.SetCancelOrFinish(0);
        }

        internal ClientCard AddNewCard(int ctr, int loc, int seq, int pos = 0x2)
        {
            var card = cardPool.New();
            activeCards.Add(card);
            card.Initialize(ctr, loc, seq);
            card.Position = pos;
            if (loc == (int)CardLocation.Hand)
            {
                HandCards[ctr].Add(card);
            }
            return card;
        }

        internal void Move(BinaryReader reader)
        {
            uint code = reader.ReadUInt32();
            Coordinate preCoord = reader.ReadCoordinate();
            int prePos = reader.ReadByte();
            Coordinate curCoord = reader.ReadCoordinate();
            int curPos = reader.ReadByte();
            Debug.Log(string.Format("-->move from:[{0}] to:[{1}],prePos=0x{2},curPos=0x{3}",
                preCoord, curCoord.ToString(), prePos.ToString("X"), curPos.ToString("X")));
            int reason = reader.ReadInt32();
            float duration = 0.2f;
            float waitTime = 0.1f;
            if (preCoord.Location == 0)
            {
                var tcard = AddNewCard(curCoord.Controller, curCoord.Location, curCoord.Sequence, curPos);
                tcard.SetCode(code);
                MoveCard(tcard, duration, waitTime);
                return;
            }
            else if (curCoord.Location == 0)
            {
                var tcard = GetCard(preCoord);
                if (code != 0)
                {
                    tcard.SetCode(code);
                }
                //todo: fading
                cardPool.Store(tcard);
                activeCards.Remove(tcard);
                return;
            }
            ClientCard card;
            if (preCoord.IsXyzMaterial())
            {
                var olcard = GetCard(preCoord.Controller, preCoord.Location & 0x7f, preCoord.Sequence);
                card = olcard.Overlay[prePos];
                card.OverlayTarget = null;
                olcard.Overlay.Remove(card);
                for (int i = 0; i < olcard.Overlay.Count; i++)
                {
                    olcard.Overlay[i].Sequence = i;
                    MoveCard(olcard.Overlay[i], duration, 0);
                }
            }
            else
            {
                card = GetCard(preCoord);
                if (code != 0 || curCoord.Location == 0x40)
                {
                    card.SetCode(code);
                }
                if ((preCoord.Location & (int)CardLocation.OnField) > 0 && (preCoord.Location != curCoord.Location))
                {
                    card.Counters.Clear();
                }
                if (preCoord.Location != curCoord.Location)
                {
                    //todo: clear equit, target
                }
                if (preCoord.Location == (int)CardLocation.Hand)
                {
                    HandCards[preCoord.Controller].RemoveAt(card.Sequence);
                    for (int i = card.Sequence; i < HandCards[preCoord.Controller].Count; ++i)
                    {
                        HandCards[preCoord.Controller][i].Sequence--;
                    }
                    ArrangeHand(preCoord.Controller, waitTime);
                }
                else if ((preCoord.Location & 0x71) > 0)
                {
                    var cards = GetCards(preCoord.Controller, (CardLocation)preCoord.Location);
                    foreach (var c in cards)
                    {
                        if (c.Sequence > preCoord.Sequence)
                        {
                            c.Sequence--;
                            MoveCard(c, duration, 0);
                        }
                    }
                }
            }
            if (curCoord.IsXyzMaterial())
            {
                card.Counters.Clear();
                var target = GetCard(curCoord.Controller, curCoord.Location & 0x7f, curCoord.Sequence);
                target.Overlay.Add(card);
                card.OverlayTarget = target;
                card.Location = (int)CardLocation.Overlay;
                card.Sequence = target.Overlay.Count - 1;
                MoveCard(card, duration, waitTime);
            }
            else
            {
                switch ((CardLocation)curCoord.Location)
                {
                    case CardLocation.MonsterZone:
                        card.SetState(curCoord);
                        card.Position = curPos;
                        MoveCard(card, duration, waitTime);
                        card.Overlay.ForEach((xyz) => MoveCard(xyz, duration, 0));
                        break;
                    case CardLocation.Hand:
                        card.SetState(curCoord);
                        card.Position = curPos;
                        HandCards[curCoord.Controller].Add(card);
                        ArrangeHand(curCoord.Controller, waitTime);
                        break;
                    case CardLocation.Deck:
                    case CardLocation.Grave:
                    case CardLocation.Removed:
                    case CardLocation.Extra:
                        if (curCoord.Sequence == 0)
                        {
                            var cards = GetCards(curCoord.Controller, (CardLocation)curCoord.Location);
                            foreach (var d in cards)
                            {
                                d.Sequence++;
                                MoveCard(d, duration, 0);
                            }
                        }
                        card.SetState(curCoord);
                        card.Position = curPos;
                        if (preCoord.Location == curCoord.Location)
                        {
                            SetCardTransform(card);
                            card.AddAnimation(card.ChangeSequence(), duration);
                        }
                        else
                        {
                            MoveCard(card, duration, waitTime);
                        }
                        break;
                    default:
                        card.SetState(curCoord);
                        card.Position = curPos;
                        MoveCard(card, duration, waitTime);
                        break;
                }
            }
        }

        internal void InitField(int controller, int deckCount, int extraCount)
        {
            for (int i = 0; i < deckCount; ++i)
            {
                InPlace(AddNewCard(controller, (int)CardLocation.Deck, i, 0x2));
            }
            for (int i = 0; i < extraCount; ++i)
            {
                InPlace(AddNewCard(controller, (int)CardLocation.Extra, i, 0x2));
            }
        }

        internal ClientCard GetCard(Coordinate coord, int subSeq = 0)
        {
            return GetCard(coord.Controller, coord.Location, coord.Sequence, subSeq);
        }

        internal ClientCard GetCard(int c, int l, int s, int subSeq = 0)
        {
            if ((l & (int)CardLocation.Overlay) > 0)
            {
                return GetCard(c, l & 0x7f, s).Overlay[subSeq];
            }
            var target = activeCards.Find((card) => card.Controller == c && card.Location == l && card.Sequence == s);
            if (target != null)
            {
                return target;
            }
            Debug.LogAssertion(string.Format("-->getcard==null[,coord={0},{1},{2},subseq:{3}", c, (CardLocation)l, s, subSeq));
            return null;
        }

        internal void UpdateFieldCard(int player, int location, BinaryReader reader)
        {
            CardLocation loc = (CardLocation)location;
            List<ClientCard> list = GetCards(player, loc, true);
            if (loc != CardLocation.MonsterZone && loc != CardLocation.SpellZone)
            {
                foreach (var card in list)
                {
                    int len = reader.ReadInt32();
                    if (len > 8)
                    {
                        long pos = reader.BaseStream.Position;
                        card.UpdateInfo(reader);
                        reader.BaseStream.Position = pos + len - 4;
                    }
                }
            }
            else
            {
                int count = 7;
                for (int i = 0; i < count; ++i)
                {
                    sequencePointer.Add(i, null);
                }
                foreach (var card in list)
                {
                    sequencePointer[card.Sequence] = card;
                }
                for (int i = 0; i < count; ++i)
                {
                    int len = reader.ReadInt32();
                    if (len > 8)
                    {
                        long pos = reader.BaseStream.Position;
                        sequencePointer[i].UpdateInfo(reader);
                        reader.BaseStream.Position = pos + len - 4;
                    }
                }
                sequencePointer.Clear();
            }
        }

        internal List<ClientCard> GetCards(int player, CardLocation location, bool sortBySeq = false)
        {
            if (location == CardLocation.Hand)
            {
                return HandCards[player];
            }
            var list = activeCards.FindAll((c) => c.Location == (int)location && c.Controller == player);
            if (sortBySeq)
            {
                list.Sort(ClientCard.SequenceCompare);
            }
            return list;
        }

        internal void ClearCommands()
        {
            if (InteractiveCards.Count > 0)
            {
                if (spreadingTranforms.ContainsKey(InteractiveCards[0].Transform))
                {
                    EndQuery();
                }
                foreach (var c in InteractiveCards)
                {
                    c.ClearCommand();
                }
                InteractiveCards.Clear();
            }
        }

        internal void Clear()
        {
            MainGame.Instance.FrameActions.Remove(Update);
            PlaceIndicator.Clear();
            fieldObject.transform.localScale = Vector3.zero;
            ClearChains();
            Attacker = null;
            sequencePointer.Clear();
            cardPool.Store(activeCards);
            activeCards.Clear();
            HandCards[0].Clear();
            HandCards[1].Clear();
            InteractiveCards.Clear();
            ResetSelection();
        }

        internal void ClearChains()
        {
            chainPool.Store(CurrentChains);
            CurrentChains.Clear();
        }

        internal void MoveCard(ClientCard card, float duration, float waitTime)
        {
            SetCardTransform(card);
            card.AddAnimation(card.MoveReal(duration), waitTime);
        }

        internal void InPlace(ClientCard card)
        {
            SetCardTransform(card);
            card.Transform.position = card.RealPosition;
            card.Transform.rotation = card.RealRotation;
        }

        internal void ResizeGroup(int player, int deckCount, uint topCode, List<uint> handCodes, List<uint> exCodes)
        {
            var resized = activeCards.FindAll((c) => c.Controller == player && (c.Location & 0x43) > 0);
            var stage1 = new List<ClientCard>(resized);
            List<ClientCard> disposed = null;
            int tarCount = handCodes.Count + deckCount + exCodes.Count;
            if (resized.Count < tarCount)
            {
                while (resized.Count < tarCount)
                {
                    var card = cardPool.New();
                    card.Transform.position = new Vector3(0, 0, -70 * (-2 * player + 1));
                    resized.Add(card);
                    activeCards.Add(card);
                }
            }
            else
            {
                disposed = new List<ClientCard>(resized.Count - tarCount);
                while (resized.Count > tarCount)
                {
                    var last = resized[resized.Count - 1];
                    disposed.Add(last);
                    activeCards.Remove(last);
                    resized.RemoveAt(resized.Count - 1);
                }
            }
            for (int i = 0; i < deckCount; ++i)
            {
                resized[i].SetState(player, (int)CardLocation.Deck, i);
                resized[i].SetCode(0);
            }
            resized[0].SetCode(topCode);
            for (int i = deckCount; i < deckCount + exCodes.Count; ++i)
            {
                resized[i].SetState(player, (int)CardLocation.Extra, i - deckCount);
                resized[i].SetCode(exCodes[i - deckCount]);
            }
            HandCards[player].Clear();
            for (int i = deckCount + exCodes.Count; i < resized.Count; ++i)
            {
                resized[i].SetState(player, (int)CardLocation.Hand, i - deckCount - exCodes.Count);
                HandCards[player].Add(resized[i]);
                resized[i].SetCode(handCodes[i - deckCount - exCodes.Count]);
            }
            Animator.Instance.Play(new DuelAnimation(TagSwap(player, stage1, disposed, resized), 0.2f));
        }

        internal IEnumerator TagSwap(int player, List<ClientCard> stage1, List<ClientCard> disposed, List<ClientCard> stage2)
        {
            List<IEnumerator> anims = new List<IEnumerator>(stage1.Count);
            foreach (var card in stage1)
            {
                anims.Add(card.Transform.MoveAndRotateTo(new Vector3(0, 0, -70 * (-2 * player + 1)),
                    FieldInfo.FieldRotations[player][CardPosition.FaceDown], 0.2f));
            }
            yield return Animator.GroupAnimationWrapper(anims);
            if (spreadingTranforms.Count > 0)
            {
                var spr = spreadingTranforms[inertiaTransform.GetChild(0)];
                if (spr.Controller == player && spr.Location == (int)CardLocation.Extra)
                {
                    EndQuery();
                }
            }
            if (disposed != null)
            {
                cardPool.Store(disposed);
            }
            foreach (var card in stage2)
            {
                card.Visualize();
                SetCardTransform(card);
                card.AddAnimation(card.MoveRealSmoothly(0.2f), 0.2f);
            }
        }

        internal void BeginChaining(Coordinate curCoor, ClientCard card)
        {
            Chain chain = chainPool.New();
            chain.ChainSequence = CurrentChains.Count + 1;
            chain.ChainCard = card;
            chain.Coordinate = curCoor;
            chain.ChainCode = card.Data.Code;
            CurrentChains.Add(chain);

            if (curCoor.Location == (int)CardLocation.Hand)
            {
                chain.Activate(Vector3.Lerp(card.RealPosition, MainGame.Instance.MainCamera.transform.position, 0.01f));
            }
            else
            {
                int factor = curCoor.Controller == 0 ? 1 : -1;
                int repeat = 0;
                for (int i = 0; i < CurrentChains.Count - 1; ++i)
                {
                    if ((curCoor.Location & 0xE) > 0)
                    {
                        if (CurrentChains[i].Coordinate.Equals(curCoor))
                        {
                            repeat += 2;
                        }
                    }
                    else if (curCoor.Location == CurrentChains[i].Coordinate.Location
                        && curCoor.Controller == CurrentChains[i].Coordinate.Controller)
                    {
                        repeat += 2;
                    }
                }
                if (curCoor.Location == (int)CardLocation.MonsterZone)
                {
                    chain.Activate(FieldInfo.MZonePositions[curCoor.Sequence] * factor + new Vector3(0, 0.1f, repeat * factor));
                }
                else if (curCoor.Location == (int)CardLocation.SpellZone)
                {
                    chain.Activate(FieldInfo.SZonePositions[curCoor.Sequence] * factor + new Vector3(0, 0.1f, repeat * factor));
                }
                else
                {
                    //todo activate at top
                    chain.Activate(FieldInfo.BasePositions[(CardLocation)curCoor.Location] * factor + new Vector3(0, 6, repeat * factor));
                }
            }
            card.AddAnimation(card.ChainingShowUp(), 0.45f);
        }

        internal void ArrangeHand(int player, float waitSeconds)
        {
            if (HandCards[player].Count == 0)
            {
                return;
            }
            HandCards[player].ForEach((c) => MoveCard(c, 0.2f, waitSeconds));
        }

        internal void ShuffleHand(int ctrler)
        {
            if (HandCards[ctrler].Count == 0)
            {
                return;
            }
            if (HandCards[ctrler].Count == 1)
            {
                SetCardTransform(HandCards[ctrler][0]);
                HandCards[ctrler][0].AddAnimation(HandCards[ctrler][0].MoveReal(0.2f), 0.1f);
                return;
            }
            foreach (var c in HandCards[ctrler])
            {
                SetCardTransform(c);
                c.AddAnimation(c.ShuffleHandMove(0.3f), 0.15f);
            }
        }

        internal void ShuffleSet(List<ClientCard> cards)
        {
            foreach (var card in cards)
            {
                SetCardTransform(card);
                card.AddAnimation(card.ShuffleSetMove());
                if (card.Overlay.Count > 0)
                {
                    foreach (var c in card.Overlay)
                    {
                        SetCardTransform(c);
                        c.AddAnimation(c.MoveReal(0.2f), 0);
                    }
                }
            }
        }

        private void SetCardTransform(ClientCard card)
        {
            int factor = card.Controller == 0 ? 1 : -1;
            int p = card.Controller;
            CardLocation loc = (CardLocation)card.Location;
            switch (loc)
            {
                case CardLocation.Deck:
                    if (GameInfo.Instance.IsDeckReversed)
                    {
                        card.RealRotation = FieldInfo.FieldRotations[p][CardPosition.FaceUpAttack];
                    }
                    else
                    {
                        card.RealRotation = FieldInfo.FieldRotations[p][CardPosition.FaceDownAttack];
                    }
                    card.RealPosition = (FieldInfo.BasePositions[loc] + new Vector3(0, card.Sequence * 0.1f * factor, 0)) * factor;
                    break;
                case CardLocation.Grave:
                    card.RealRotation = FieldInfo.FieldRotations[p][CardPosition.FaceUpAttack];
                    card.RealPosition =
                       (FieldInfo.BasePositions[loc] + new Vector3(0, card.Sequence * 0.1f * factor, 0)) * factor;
                    break;
                case CardLocation.Extra:
                    if ((card.Position & (int)CardPosition.FaceUp) > 0)
                    {
                        card.RealRotation = FieldInfo.FieldRotations[p][CardPosition.FaceUpAttack];
                    }
                    else
                    {
                        card.RealRotation = FieldInfo.FieldRotations[p][CardPosition.FaceDownAttack];
                    }
                    card.RealPosition =
                        (FieldInfo.BasePositions[loc] + new Vector3(0, card.Sequence * 0.1f * factor, 0)) * factor;
                    break;
                case CardLocation.Removed:
                    card.RealRotation = FieldInfo.FieldRotations[p][(CardPosition)card.Position];
                    card.RealPosition =
                        (FieldInfo.BasePositions[loc] + new Vector3(0, card.Sequence * 0.1f * factor, 0)) * factor;
                    break;
                case CardLocation.Hand:
                    if (card.Data.Code > 0)
                    {
                        card.RealRotation = FieldInfo.HandRotations[CardPosition.FaceUp];
                    }
                    else
                    {
                        card.RealRotation = FieldInfo.HandRotations[CardPosition.FaceDown];
                    }
                    float interval = 6.9f;
                    int count = HandCards[p].Count;
                    float left;
                    if (count > 10)
                    {
                        left = -31.05f * factor;
                        interval = 62.1f / (count - 1);
                        card.RealRotation = Quaternion.Euler(card.RealRotation.eulerAngles + new Vector3(0, 0.5f, 0));
                    }
                    else
                    {
                        left = -6.9f * (count - 1) * 0.5f * factor;
                    }
                    card.RealPosition = new Vector3(left + interval * card.Sequence * factor, 0, -34 * factor);
                    break;
                case CardLocation.MonsterZone:
                    card.RealRotation = FieldInfo.FieldRotations[p][(CardPosition)card.Position];
                    card.RealPosition = FieldInfo.MZonePositions[card.Sequence] * factor;
                    break;
                case CardLocation.SpellZone:
                    card.RealRotation = FieldInfo.FieldRotations[p][(CardPosition)card.Position];
                    card.RealPosition = FieldInfo.SZonePositions[card.Sequence] * factor;
                    break;
                case CardLocation.FZone:
                    card.RealRotation = FieldInfo.FieldRotations[p][(CardPosition)card.Position];
                    card.RealPosition = new Vector3(-30.45f, 0, -15.9f) * factor;
                    break;
                case CardLocation.Overlay:
                    if (!(card.OverlayTarget == null || card.OverlayTarget.Location != (int)CardLocation.MonsterZone))
                    {
                        int xyzFactor = card.OverlayTarget.Controller == 0 ? 1 : -1;
                        card.RealPosition = (FieldInfo.MZonePositions[card.OverlayTarget.Sequence] +
                            new Vector3(0.3f, -0.1f * xyzFactor, 0) * (card.Sequence + 1)) * xyzFactor;
                    }
                    card.RealRotation = FieldInfo.FieldRotations[p][CardPosition.FaceUpAttack];
                    break;
                default:
                    Debug.LogWarning(loc);
                    card.RealPosition = Vector3.zero;
                    card.RealRotation = Quaternion.Euler(Vector3.zero);
                    break;
            }
        }

        internal void RefreshAll()
        {
            activeCards.ForEach(InPlace);
        }

        internal void Show()
        {
            if (fieldObject == null)
            {
                fieldObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Field")) as GameObject;
            }
            MainGame.Instance.FrameActions.Add(Update);
            fieldObject.transform.localScale = Vector3.one;
        }
    }
}
