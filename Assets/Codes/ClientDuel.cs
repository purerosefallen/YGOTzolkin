using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using YGOTzolkin.Elements;
using YGOTzolkin.Service;
using YGOTzolkin.Utility;
using Random = UnityEngine.Random;
using static YGOTzolkin.Service.DataService;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin
{
    public struct Coordinate
    {
        public int Controller;
        public int Location;
        public int Sequence;
        public Coordinate(int c, int l, int s)
        {
            Controller = c;
            Location = l;
            Sequence = s;
        }
        public override string ToString()
        {
            return string.Format("ctr:{0},loc:{1},seq:{2}", Controller, (CardLocation)Location, Sequence);
        }
        public bool IsXyzMaterial()
        {
            return (Location & 0x80) > 0;
        }
        public bool Equals(Coordinate coord)
        {
            return Controller == coord.Controller && Sequence == coord.Sequence && Location == coord.Location;
        }
    }

    class ClientDuel
    {
        public ClientDuel()
        {
            gmHandlers = new Dictionary<GameMessage, MessageHandler>()
            {
                {GameMessage.Retry,OnRetry },
                {GameMessage.Hint,OnHint },
                {GameMessage.Waiting,OnWainting },
                {GameMessage.Start,OnStart },
                {GameMessage.Win,OnWin },
                {GameMessage.UpdateData,OnUpdateData },
                {GameMessage.UpdateCard,OnUpdateCard },
                {GameMessage.RequestDeck,NotImplemented },
                {GameMessage.SelectBattleCmd,OnSelectBattleCmd },
                {GameMessage.SelectIdleCmd,OnSelectIdleCmd },
                {GameMessage.SelectEffectYN,OnSelectEffectYN },
                {GameMessage.SelectYesNo,OnSelectYesNo },
                {GameMessage.SelectOption,OnSelectOption },
                {GameMessage.SelectCard,OnSelectCard },
                {GameMessage.SelectChain,OnSelectChain },
                {GameMessage.SelectPlace,OnDisfield },
                {GameMessage.SelectPosition,OnSelectPosition },
                {GameMessage.SelectTribute,OnSelectTribute },
                {GameMessage.SortChain,OnSortChain },
                {GameMessage.SelectCounter,OnSelectCounter },
                {GameMessage.SelectSum,OnSelectSum },
                {GameMessage.SelectDisfield,OnDisfield },
                {GameMessage.SortCard,OnSortChain },
                {GameMessage.SelectUnSelectCard,OnSelectUnSelectCard },
                {GameMessage.ConfirmDecktop,OnConfirmDeckTop },
                {GameMessage.ConfirmCards,OnConfirmCards },
                {GameMessage.ShuffleDeck,OnShuffleDeck },
                {GameMessage.ShuffleHand,OnShuffleHand },
                {GameMessage.RefreshDeck,NotImplemented },
                {GameMessage.SwapGraveDeck,OnSwapGraveDeck },
                {GameMessage.ShuffleSetCard,OnShuffleSetCard },
                {GameMessage.ReverseDeck,OnReverseDeck },
                {GameMessage.DeckTop,OnDeckTop },
                {GameMessage.ShuffleExtra,OnShuffleExtra },
                {GameMessage.NewTurn,OnNewTurn },
                {GameMessage.NewPhase,OnNewPhase },
                {GameMessage.ConfirmExtraTop,OnConfirmExtraTop },
                {GameMessage.Move,OnMove },
                {GameMessage.PosChange,OnPosChange },
                {GameMessage.Set,OnSet },
                {GameMessage.Swap,OnSwap },
                {GameMessage.FieldDisabled,OnFieldDisabled },
                {GameMessage.Summoning,OnSummoning },
                {GameMessage.Summoned,OnSummoned },
                {GameMessage.SpSummoning,OnSpSummoning },
                {GameMessage.SpSummoned,OnSpSummoned },
                {GameMessage.FlipSummoning,OnFlipSummoning },
                {GameMessage.FlipSummoned,OnFlipSummoned },
                {GameMessage.Chaining,OnChaining },
                {GameMessage.Chained,OnChained },
                {GameMessage.ChainSolving,OnChainSolving },
                {GameMessage.ChainSolved,OnChainSolved },
                {GameMessage.ChainEnd,OnChainEnd },
                {GameMessage.ChainNegated,OnChainDisabled },
                {GameMessage.ChainDisabled,OnChainDisabled },
                {GameMessage.CardSelected,NotImplemented },
                {GameMessage.RandomSelected,OnRandomSelected },
                {GameMessage.BecomeTarget,OnBecomeTarget },
                {GameMessage.Draw,OnDraw },
                {GameMessage.Damage,OnDamage },
                {GameMessage.Recover,OnRecover },
                {GameMessage.Equip,OnEquip },
                {GameMessage.LpUpdate,OnLpUpadte },
                {GameMessage.Unequip,OnUnequip },
                {GameMessage.CardTarget,OnCardTarget },
                {GameMessage.CancelTarget,OnCancelTarget },
                {GameMessage.PayLpCost,OnPayLpCost },
                {GameMessage.AddCounter,OnAddCounter },
                {GameMessage.RemoveCounter,OnRemoveCounter },
                {GameMessage.Attack,OnAttack },
                {GameMessage.Battle,OnBattle },
                {GameMessage.AttackDiabled,OnAttackDisabled },
                {GameMessage.DamageStepStart,NotImplemented },
                {GameMessage.DamageStepEnd,NotImplemented },
                {GameMessage.MissedEffect,OnMissedEffect },
                {GameMessage.BeChainTarget,NotImplemented },
                {GameMessage.CreateRelation,NotImplemented },
                {GameMessage.ReleaseRelation,NotImplemented },
                {GameMessage.TossCoin,OnTossCoin },
                {GameMessage.TossDice,OnTossDice },
                {GameMessage.RockPaperScissors,OnRockPaperScissors },
                {GameMessage.HandResult,OnHandResult },
                {GameMessage.AnnounceRace,OnAnnounceRace },
                {GameMessage.AnnounceAttribute,OnAnnounceAttribute },
                {GameMessage.AnnounceCard,OnAnnounceCard },
                {GameMessage.AnnounceNumber,OnAnnounceNumber },
                {GameMessage.AnnounceCardFilter,NotImplemented },
                {GameMessage.CardHint,OnCardHint },
                {GameMessage.TagSwap,OnTagSwap },
                {GameMessage.ReloadField,OnReloadField },
                {GameMessage.AiName,NotImplemented },
                {GameMessage.ShowHint,NotImplemented },
                {GameMessage.PlayerHint,OnPlayerHint },
                {GameMessage.MatchKill,OnMatchKill },
                {GameMessage.CustomMsg,NotImplemented },
            };
            stocHanders = new Dictionary<SToCMessage, MessageHandler>
            {
                {SToCMessage.ErrorMessage,OnError },
                {SToCMessage.SelectHand,OnSelectHand },
                {SToCMessage.SelectTp,OnSelectTp },
                {SToCMessage.HandResult,OnSToCHandResult },
                {SToCMessage.TpResult,NotImplemented },
                {SToCMessage.ChangeSide,OnChangeSide },
                {SToCMessage.WaitingSide,NotImplemented },
                {SToCMessage.JoinGame,OnJoinGame },
                {SToCMessage.TypeChange,OnTypeChange },
                {SToCMessage.DuelStart,OnDuelStart },
                {SToCMessage.DuelEnd,OnDuelEnd },
                {SToCMessage.Replay,OnReplay },
                {SToCMessage.TimeLimit,OnTimeLimit },
                {SToCMessage.Chat,OnChat },
                {SToCMessage.HsPlayerEnter,OnPlayerEnter },
                {SToCMessage.HsPlayerChange,OnPlayerChange },
                {SToCMessage.HsWatchChange,OnHsWatchChange },
            };
            isWorking = false;
            canGetNextMessage = true;
            blockedMsg = null;
        }
        internal bool IsChainForced { get; private set; }

        private delegate void MessageHandler(BinaryReader reader);
        private readonly Dictionary<GameMessage, MessageHandler> gmHandlers;
        private readonly Dictionary<SToCMessage, MessageHandler> stocHanders;

        private readonly Field field = MainGame.Instance.Field;
        private bool isWorking;
        private bool canGetNextMessage;
        private Action blockedMsg;

        internal void Start()
        {
            isWorking = true;
        }

        internal void Update()
        {
            if (!isWorking)
            {
                return;
            }
            NetworkService.InstantQueue.TryTake(out byte[] insdata);
            if (!(insdata == null || insdata.Length == 0))
            {
                BinaryReader reader = new BinaryReader(new MemoryStream(insdata));
                var stoc = (SToCMessage)reader.ReadByte();
                stocHanders[stoc](reader);
            }
            if (Animator.Instance.IsBlockingStopped && !field.IsConfirming)
            {
                canGetNextMessage = true;
                blockedMsg?.Invoke();
                blockedMsg = null;
            }
            if (canGetNextMessage)
            {
                NetworkService.DelayedQueue.TryTake(out byte[] data);
                if (!(data == null || data.Length == 0))
                {
                    BinaryReader reader = new BinaryReader(new MemoryStream(data));
                    var stoc = (SToCMessage)reader.ReadByte();
                    if (stoc != SToCMessage.GameMessage)
                    {
                        if (stoc == SToCMessage.TimeLimit)
                        {
                            canGetNextMessage = false;
                            blockedMsg = new Action(() => stocHanders[stoc](reader));
                        }
                        else
                        {
                            Debug.Log("SToC." + stoc.ToString());
                            stocHanders[stoc](reader);
                        }
                    }
                    else
                    {
                        GameMessage message = (GameMessage)reader.ReadByte();
                        if (message != GameMessage.UpdateData && message != GameMessage.Waiting)
                        {
                            Debug.Log("GameMessage." + message.ToString());
                        }
                        if (NeedBlocking(message))
                        {
                            canGetNextMessage = false;
                            blockedMsg = new Action(() =>
                            {
                                GameInfo.Instance.CurrentMessage = message;
                                gmHandlers[message](reader);
                            });
                        }
                        else
                        {
                            GameInfo.Instance.CurrentMessage = message;
                            gmHandlers[message](reader);
                        }
                    }
                }
            }
        }

        private bool NeedBlocking(GameMessage message)
        {
            if (/*message == GameMessage.Start ||*/ message == GameMessage.ConfirmDecktop
                || message == GameMessage.ConfirmCards || message == GameMessage.ShuffleDeck
                || message == GameMessage.ShuffleHand || message == GameMessage.SwapGraveDeck
                || message == GameMessage.DeckTop || message == GameMessage.ShuffleExtra
                || message == GameMessage.NewTurn || message == GameMessage.NewPhase
                || message == GameMessage.ConfirmExtraTop || message == GameMessage.Move
                || message == GameMessage.PosChange || message == GameMessage.Swap
                || message == GameMessage.FlipSummoning || message == GameMessage.Draw
                || message == GameMessage.Chaining || message == GameMessage.ChainSolving
                || message == GameMessage.BecomeTarget || message == GameMessage.PayLpCost
                || message == GameMessage.Damage || message == GameMessage.Hint || message == GameMessage.TagSwap
                || message == GameMessage.ReverseDeck || message == GameMessage.AddCounter || message == GameMessage.RemoveCounter
                || message == GameMessage.ShuffleSetCard)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnDuelEnd(BinaryReader reader)
        {
            MainGame.Instance.DuelWindow.Hide();
            MainGame.Instance.HintBox.ShowHint(SysString(1500), 0.5f);
            MainGame.Instance.ChatWindow.Hide();
            MainGame.Instance.ServerWindow.Show();
            MainGame.Instance.Descriptor.Hide();
            field.Clear();
            isWorking = false;
        }

        private void OnChangeSide(BinaryReader reader)
        {
            MainGame.Instance.DuelWindow.Hide();
            field.Clear();
            MainGame.Instance.DeckBuilder.ShowSideChange();
        }

        private void OnReplay(BinaryReader reader)
        {
            byte[] repdata = reader.ReadBytes((int)reader.BaseStream.Length - 1);
            FileStream stream = new FileStream(Config.ReplayPath + "temp.yrp", FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(repdata);
        }

        private void OnSelectUnSelectCard(BinaryReader reader)
        {
            field.EndQuery();
            reader.ReadByte();
            field.Finishable = reader.ReadByte() != 0;
            field.Cancelabel = reader.ReadByte() != 0;
            field.SelectMin = reader.ReadByte();
            field.SelectMax = reader.ReadByte();
            byte count = reader.ReadByte();
            bool spread = false;
            Debug.Log(string.Format("-->selunsel, min={0}, max={1}, count={2}", field.SelectMin, field.SelectMax, count));
            if (GameInfo.Instance.SelectHint > 0)
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(string.Format("{0}({1}-{2})",
                    GetDescription(GameInfo.Instance.SelectHint), field.SelectMin, field.SelectMax));
                GameInfo.Instance.SelectHint = 0;
            }
            else
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(string.Format("{0}({1}-{2})",
                    SysString(560), field.SelectMin, field.SelectMax));
            }
            for (byte i = 0; i < count; ++i)
            {
                uint code = reader.ReadUInt32();
                var coord = reader.ReadCoordinate();
                int ss = reader.ReadByte();
                var card = field.GetCard(coord, ss);
                if (code != 0)
                {
                    card.SetCode(code);
                }
                card.SelectSequence = i;
                field.SelectableCards.Add(card);
                card.Selected = false;
                card.Selectable = true;
                if ((coord.Location & 0xf1) > 0)
                {
                    spread = true;
                }
                Debug.Log(string.Format("-->00{0}: {1} is selectabel,", card.SelectSequence, GetName(card.Data.Code)));
            }
            byte count2 = reader.ReadByte();
            for (byte i = count; i < count + count2; ++i)
            {
                uint code = reader.ReadUInt32();
                var coord = reader.ReadCoordinate();
                int ss = reader.ReadByte();
                var card = field.GetCard(coord, ss);
                if (code != 0)
                {
                    card.SetCode(code);
                }
                card.SelectSequence = i;
                field.SelectableCards.Add(card);
                card.Selected = true;
                card.Selectable = true;
                if ((coord.Location & 0xf1) > 0)
                {
                    spread = true;
                }
                Debug.Log(string.Format("-->00{0}: {1} is selectabel, selected", card.SelectSequence, GetName(card.Data.Code)));
            }
            if (field.Finishable)
            {
                MainGame.Instance.DuelWindow.SetCancelOrFinish(2);
            }
            if (field.Cancelabel)
            {
                MainGame.Instance.DuelWindow.SetCancelOrFinish(1);
            }
            if (spread)
            {
                field.SpreadSelection();
            }
        }

        private void OnSelectSum(BinaryReader reader)
        {
            field.EndQuery();
            field.Overflowable = reader.ReadByte() != 0;
            reader.ReadByte();
            field.SelectSumVal = reader.ReadInt32();
            field.SelectMin = reader.ReadByte();
            field.SelectMax = reader.ReadByte();
            field.SelectSumFixedCount = reader.ReadByte();
            bool needSpread = false;
            Debug.Log(string.Format("-->select sum: min={0}, max={1}, sumval={2}, overflowabel={3}",
                field.SelectMin, field.SelectMax, field.SelectSumVal, field.Overflowable));
            if (GameInfo.Instance.SelectHint > 0)
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(string.Format("{0}({1})",
                    GetDescription(GameInfo.Instance.SelectHint), field.SelectSumVal));
                GameInfo.Instance.SelectHint = 0;
            }
            else
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(string.Format("{0}({1})", SysString(560), field.SelectSumVal));
            }
            for (int i = 0; i < field.SelectSumFixedCount; ++i)
            {
                uint code = reader.ReadUInt32();
                var card = field.GetCard(reader.ReadCoordinate());
                if (code != 0)
                {
                    card.SetCode(code);
                }
                card.SelectionParam = reader.ReadInt32();
                card.SelectSequence = 0;
                card.Selectable = false;
                card.Selected = true;
                field.SelectedCards.Add(card);
                Debug.Log(string.Format("-->000: {0} is unselectable, selected, param1={1},param2={2}, fixed selected",
                    GetName(card.Data.Code), card.SelectionParam & 0xffff, card.SelectionParam >> 16));
            }
            int count = reader.ReadByte();
            for (byte i = 0; i < count; ++i)
            {
                uint code = reader.ReadUInt32();
                var card = field.GetCard(reader.ReadCoordinate());
                if (code != 0)
                {
                    card.SetCode(code);
                }
                card.SelectionParam = reader.ReadInt32();
                card.SelectSequence = i;
                card.Selectable = true;
                card.Selected = false;
                field.SelectableCards.Add(card);
                Debug.Log(string.Format("-->111: {0} is Selectable, param1={1},param2={2}, to be selected",
                    GetName(card.Data.Code), card.SelectionParam & 0xffff, card.SelectionParam >> 16));
                if ((card.Location & 0xf1) > 0)
                {
                    needSpread = true;
                }
            }
            if (needSpread)
            {
                field.SpreadSelection();
            }
            field.UpdateSelectSum(needSpread);
        }

        private void OnSelectCounter(BinaryReader reader)
        {
            field.EndQuery();
            reader.ReadByte();
            field.CounterToBeSelected = reader.ReadUInt16();
            field.SelectMin = field.SelectMax = reader.ReadInt16();
            int count = reader.ReadByte();
            for (int i = 0; i < count; ++i)
            {
                reader.ReadInt32();
                var card = field.GetCard(reader.ReadCoordinate());
                int p = reader.ReadInt16();
                field.SelectableCards.Add(card);
                card.Selectable = true;
                card.Selected = false;
                card.SelectionParam = (p << 16) | p;
            }
            MainGame.Instance.DuelWindow.ShowSelectHint(
                string.Format(SysString(204), field.SelectMin, CounterName(field.CounterToBeSelected)));
        }

        private void OnSelectTribute(BinaryReader reader)
        {
            field.EndQuery();
            reader.ReadByte();
            field.Cancelabel = reader.ReadByte() != 0;
            field.SelectMin = reader.ReadByte();
            field.SelectMax = reader.ReadByte();
            int count = reader.ReadByte();
            if (GameInfo.Instance.SelectHint > 0)
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(string.Format("{0}({1}-{2})",
                    GetDescription(GameInfo.Instance.SelectHint), field.SelectMin, field.SelectMax));
                GameInfo.Instance.SelectHint = 0;
            }
            else
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(string.Format("{0}({1}-{2})",
                    SysString(531), field.SelectMin, field.SelectMax));
            }
            for (byte i = 0; i < count; ++i)
            {
                uint code = reader.ReadUInt32();
                var coord = reader.ReadCoordinate();
                byte t = reader.ReadByte();
                var card = field.GetCard(coord);
                if (code > 0)
                {
                    card.SetCode(code);
                }
                field.SelectableCards.Add(card);
                card.SelectionParam = t;
                card.SelectSequence = i;
                card.Selectable = true;
            }
            if (field.Cancelabel)
            {
                MainGame.Instance.DuelWindow.SetCancelOrFinish(1);
            }
        }

        private void OnSelectPosition(BinaryReader reader)
        {
            reader.ReadByte();
            uint code = reader.ReadUInt32();
            int positions = reader.ReadByte();
            if (positions == 0x1 || positions == 0x2 || positions == 0x4 || positions == 0x8)
            {
                MainGame.Instance.SendCToSResponse(positions);
                return;
            }
            MainGame.Instance.ImageSelector.SelectPosition(code, positions);
        }

        private void OnTagSwap(BinaryReader reader)
        {
            int p = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            int deckc = reader.ReadByte();
            int exc = reader.ReadByte();
            int pc = reader.ReadByte();
            int handc = reader.ReadByte();
            uint topcode = reader.ReadUInt32();
            List<uint> hcodes = new List<uint>(handc);
            List<uint> excodes = new List<uint>(exc);
            for (int i = 0; i < handc; ++i)
            {
                hcodes.Add(reader.ReadUInt32());
            }
            for (int i = 0; i < exc; ++i)
            {
                excodes.Add((reader.ReadUInt32() & 0x7fffffff));
            }
            field.ResizeGroup(p, deckc, topcode, hcodes, excodes);
            //todo extra pcount
        }

        private void OnMatchKill(BinaryReader reader)
        {
            GameInfo.Instance.MatchKill = reader.ReadInt32();
        }

        private void OnAnnounceNumber(BinaryReader reader)
        {
            reader.ReadByte();
            int count = reader.ReadByte();
            if (count == 1)
            {
                MainGame.Instance.HintBox.ShowHint(SysString(215) + reader.ReadInt32(), 0.5f);
                MainGame.Instance.SendCToSResponse(0);
                return;
            }
            List<int> nums = new List<int>();
            for (int i = 0; i < count; ++i)
            {
                nums.Add(reader.ReadInt32());
            }
            if (GameInfo.Instance.SelectHint > 0)
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(GetDescription(GameInfo.Instance.SelectHint));
            }
            else
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(SysString(565));
            }
            GameInfo.Instance.SelectHint = 0;
            MainGame.Instance.ToggleSelector.SelectNumber(nums);
        }

        private void OnAnnounceCard(BinaryReader reader)
        {
            reader.ReadByte();
            int count = reader.ReadByte();
            List<int> opcodes = new List<int>(count);
            for (int i = 0; i < count; ++i)
            {
                opcodes.Add(reader.ReadInt32());
            }
            if (GameInfo.Instance.SelectHint > 0)
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(GetDescription(GameInfo.Instance.SelectHint));
                GameInfo.Instance.SelectHint = 0;
            }
            else
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(GetDescription(564));
            }
            MainGame.Instance.AnnounceWindow.AnnounceName(opcodes);
        }

        private void OnAnnounceAttribute(BinaryReader reader)
        {
            reader.ReadByte();
            int count = reader.ReadByte();
            int available = reader.ReadInt32();
            MainGame.Instance.ToggleSelector.SelectAttribute(count, available);
            if (GameInfo.Instance.SelectHint > 0)
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(GetDescription(GameInfo.Instance.SelectHint));
                GameInfo.Instance.SelectHint = 0;
            }
            else
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(SysString(563));
            }
        }

        private void OnAnnounceRace(BinaryReader reader)
        {
            reader.ReadByte();
            int count = reader.ReadByte();
            int available = reader.ReadInt32();
            MainGame.Instance.ToggleSelector.SelectRace(count, available);
            if (GameInfo.Instance.SelectHint > 0)
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(GetDescription(GameInfo.Instance.SelectHint));
                GameInfo.Instance.SelectHint = 0;
            }
            else
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(SysString(563));
            }
        }

        private void OnHandResult(BinaryReader reader)
        {
            int res = reader.ReadByte();
        }

        private void OnRockPaperScissors(BinaryReader reader)
        {
            MainGame.Instance.ImageSelector.SelectHand();
        }

        private void OnTossDice(BinaryReader reader)
        {
            reader.ReadByte();
            int count = reader.ReadByte();
            StringBuilder builder = new StringBuilder(SysString(1624));
            for (int i = 0; i < count; ++i)
            {
                int res = reader.ReadByte();
                builder.Append("[").Append(res).Append("]");
            }
            MainGame.Instance.HintBox.ShowHint(builder.ToString(), 0.5f);
        }

        private void OnTossCoin(BinaryReader reader)
        {
            reader.ReadByte();
            int count = reader.ReadByte();
            StringBuilder builder = new StringBuilder(SysString(1623));
            for (int i = 0; i < count; ++i)
            {
                int res = reader.ReadByte();
                builder.Append("[").Append(res).Append("]");
            }
            MainGame.Instance.HintBox.ShowHint(builder.ToString(), 0.5f);
        }

        private void OnMissedEffect(BinaryReader reader)
        {
            reader.ReadInt32();
            MainGame.Instance.HintBox.ShowHint(string.Format(SysString(1622), GetName(reader.ReadUInt32())), 0.5f);
        }

        private void OnAttackDisabled(BinaryReader reader)
        {
            GameInfo.Instance.EventTiming =
                string.Format(SysString(1621), GetName(field.Attacker.Data.Code));
        }

        private void OnBattle(BinaryReader reader)
        {
            var cd1 = reader.ReadCoordinate();
            reader.ReadByte();
            int aatk = reader.ReadInt32();
            int adef = reader.ReadInt32();
            var cd2 = reader.ReadCoordinate();
            reader.ReadByte();
            int datk = reader.ReadInt32();
            int ddef = reader.ReadInt32();
            reader.ReadByte();
            //todo
        }

        private void OnAttack(BinaryReader reader)
        {
            var cd1 = reader.ReadCoordinate();
            reader.ReadByte();
            var cd2 = reader.ReadCoordinate();
            reader.ReadByte();
            field.Attacker = field.GetCard(cd1);
            if (cd2.Location == 0)
            {
                GameInfo.Instance.EventTiming = string.Format(SysString(1620), GetName(field.Attacker.Data.Code));
            }
            else
            {
                var tar = field.GetCard(cd2);
                GameInfo.Instance.EventTiming = string.Format(SysString(1619), GetName(field.Attacker.Data.Code),
                    GetName(tar.Data.Code));
            }
        }

        private void OnRemoveCounter(BinaryReader reader)
        {
            uint type = reader.ReadUInt16();
            var coor = reader.ReadCoordinate();
            int count = reader.ReadInt16();
            var card = field.GetCard(coor);
            card.Counters[type] -= count;
            if (card.Counters[type] <= 0)
            {
                card.Counters.Remove(type);
            }
            MainGame.Instance.HintBox.ShowHint(string.Format(SysString(1618), GetName(card.Data.Code),
                count, CounterName(type)), 0.6f);
        }


        private void OnAddCounter(BinaryReader reader)
        {
            uint type = reader.ReadUInt16();
            var coor = reader.ReadCoordinate();
            int count = reader.ReadInt16();
            var card = field.GetCard(coor);
            if (card.Counters.ContainsKey(type))
            {
                card.Counters[type] += count;
            }
            else
            {
                card.Counters.Add(type, count);
            }
            MainGame.Instance.HintBox.ShowHint(string.Format(SysString(1617), GetName(card.Data.Code), count, CounterName(type)), 0.6f);
        }

        private void OnPayLpCost(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            int cost = reader.ReadInt32();
            GameInfo.Instance.Lp[player] -= cost;
            MainGame.Instance.DuelWindow.LpChange(cost, player);
        }

        private void OnCancelTarget(BinaryReader reader)
        {
            var cd1 = reader.ReadCoordinate();
            reader.ReadByte();
            var cd2 = reader.ReadCoordinate();
            reader.ReadByte();
            field.GetCard(cd1).Untarget(field.GetCard(cd2));
        }

        private void OnCardTarget(BinaryReader reader)
        {
            var cd1 = reader.ReadCoordinate();
            reader.ReadByte();
            var cd2 = reader.ReadCoordinate();
            reader.ReadByte();
            field.GetCard(cd1).Target(field.GetCard(cd2));
        }

        private void OnUnequip(BinaryReader reader)
        {
            var cd = reader.ReadCoordinate();
            field.GetCard(cd).Unequip();
        }

        private void OnLpUpadte(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            int val = reader.ReadInt32();
            GameInfo.Instance.Lp[player] = val;
            MainGame.Instance.DuelWindow.LpUpdate(player);
        }

        private void OnEquip(BinaryReader reader)
        {
            var coord1 = reader.ReadCoordinate();
            reader.ReadByte();
            var coord2 = reader.ReadCoordinate();
            reader.ReadByte();
            field.GetCard(coord1).Equip(field.GetCard(coord2));
        }

        private void OnRecover(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            int val = reader.ReadInt32();
            GameInfo.Instance.Lp[player] += val;
            MainGame.Instance.DuelWindow.LpChange(val, player, true);
            if (player == 0)
            {
                GameInfo.Instance.EventTiming = string.Format(SysString(1615), val.ToString());
            }
            else
            {
                GameInfo.Instance.EventTiming = string.Format(SysString(1616), val.ToString());
            }
        }

        private void OnDamage(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            int val = reader.ReadInt32();
            GameInfo.Instance.Lp[player] -= val;
            if (GameInfo.Instance.Lp[player] < 0)
            {
                GameInfo.Instance.Lp[player] = 0;
            }
            MainGame.Instance.DuelWindow.LpChange(val, player);
            GameInfo.Instance.EventTiming = string.Format(SysString(1613 + (uint)player), val.ToString());
        }

        private void OnBecomeTarget(BinaryReader reader)
        {
            int count = reader.ReadByte();
            for (int i = 0; i < count; ++i)
            {
                var coor = reader.ReadCoordinate();
                reader.ReadByte();
                field.GetCard(coor).BecomeTarget();
            }
        }

        private void OnRandomSelected(BinaryReader reader)
        {
            reader.ReadByte();
            int count = reader.ReadByte();
            for (int i = 0; i < count; ++i)
            {
                Coordinate coor = reader.ReadCoordinate();
                int ss = reader.ReadByte();
                field.GetCard(coor, ss).HighLight();
            }
        }

        private void OnChainDisabled(BinaryReader reader)
        {
            int ct = reader.ReadByte();
            field.CurrentChains[ct - 1].ChainDisabled();
        }

        private void OnChainEnd(BinaryReader reader)
        {
            field.ClearChains();
        }

        private void OnChainSolved(BinaryReader reader)
        {
        }

        private void OnChainSolving(BinaryReader reader)
        {
            int ct = reader.ReadByte();
            field.CurrentChains[ct - 1].Solving();
        }

        private void OnChained(BinaryReader reader)
        {
            int c = reader.ReadByte();
            GameInfo.Instance.EventTiming = string.Format(SysString(1609),
               GetName(field.CurrentChains[field.CurrentChains.Count - 1].ChainCode));
        }

        private void OnChaining(BinaryReader reader)
        {
            uint code = reader.ReadUInt32();
            var preCoor = reader.ReadCoordinate();
            int subs = reader.ReadByte();
            var curCoor = reader.ReadCoordinate();
            int desc = reader.ReadByte();
            reader.ReadByte();
            var card = field.GetCard(preCoor, subs);
            card.SetCode(code);
            Debug.Log("-->chaining card: " + GetName(code) + " coord: " + preCoor.ToString());
            MainGame.Instance.DuelWindow.ChainingCloseup(card.Data.Code);
            field.BeginChaining(curCoor, card);
        }

        private void OnFlipSummoned(BinaryReader reader)
        {
            GameInfo.Instance.EventTiming = SysString(1608);
        }

        private void OnFlipSummoning(BinaryReader reader)
        {
            uint code = reader.ReadUInt32();
            Coordinate coor = reader.ReadCoordinate();
            int pos = reader.ReadByte();
            var card = field.GetCard(coor);
            card.SetCode(code);
            card.Position = pos;
            field.MoveCard(card, 0.2f, 0.1f);
            GameInfo.Instance.EventTiming = SysString(1607) + GetName(code);
        }

        private void OnSpSummoned(BinaryReader reader)
        {
            GameInfo.Instance.EventTiming = SysString(1606);
        }

        private void OnSpSummoning(BinaryReader reader)
        {
            uint code = reader.ReadUInt32();
            reader.ReadUInt32();
            GameInfo.Instance.EventTiming = string.Format(SysString(1605), GetName(code));
        }

        private void OnSummoned(BinaryReader reader)
        {
            GameInfo.Instance.EventTiming = SysString(1604);
        }

        private void OnSummoning(BinaryReader reader)
        {
            uint code = reader.ReadUInt32();
            reader.ReadUInt32();
            GameInfo.Instance.EventTiming =
                string.Format(SysString(1603), GetName(code));
        }

        private void OnFieldDisabled(BinaryReader reader)
        {
            uint v = reader.ReadUInt32();
            if (!GameInfo.Instance.IsFirst)
            {
                v = (v >> 16) | (v << 16);
            }
            //todo: disable some field
        }

        private void OnSet(BinaryReader reader)
        {
            reader.ReadInt64();
            GameInfo.Instance.EventTiming = SysString(1601);
        }

        private void OnMove(BinaryReader reader)
        {
            field.Move(reader);
        }

        private void OnShuffleSetCard(BinaryReader reader)
        {
            int loc = reader.ReadByte();
            int count = reader.ReadByte();
            var list = new List<ClientCard>();
            for (int i = 0; i < count; ++i)
            {
                var coor = reader.ReadCoordinate();
                reader.ReadByte();
                var card = field.GetCard(coor.Controller, loc, coor.Sequence);
                card.SetCode(0);
                list.Add(card);
            }
            for (int i = 0; i < count; ++i)
            {
                var coord = reader.ReadCoordinate();
                reader.ReadByte();
                int ps = list[i].Sequence;
                if (coord.Location > 0)
                {
                    var swp = field.GetCard(coord.Controller, loc, coord.Sequence);
                    swp.Sequence = ps;
                    list[i].Sequence = coord.Sequence;
                }
            }
            field.ShuffleSet(list);
        }

        private void OnSwapGraveDeck(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            var deck = field.GetCards(player, CardLocation.Deck);
            var grave = field.GetCards(player, CardLocation.Grave);
            foreach (var gc in grave)
            {
                if (gc.Data.IsExtra())
                {
                    gc.Location = (int)CardLocation.Extra;
                }
                else
                {
                    gc.Location = (int)CardLocation.Deck;
                }
                field.MoveCard(gc, 0.1f, 0.1f);
            }
            foreach (var dc in deck)
            {
                dc.Location = (int)CardLocation.Grave;
                field.MoveCard(dc, 0.1f, 0.1f);
            }
        }

        private void OnShuffleExtra(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            int count = reader.ReadByte();
            var cards = field.GetCards(player, CardLocation.Extra, true);
            foreach (var c in cards)
            {
                if ((c.Position & (int)CardPosition.FaceUp) == 0)
                {
                    c.AddAnimation(c.Swing(), 0.3f);
                }
            }
            foreach (var c in cards)
            {
                if ((c.Position & (int)CardPosition.FaceUp) == 0)
                {
                    c.SetCode(reader.ReadUInt32());
                }
            }
        }

        private void OnShuffleHand(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            reader.ReadByte();
            foreach (var c in field.HandCards[player])
            {
                c.SetCode(reader.ReadUInt32());
            }
            field.ShuffleHand(player);
        }

        private void OnShuffleDeck(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            field.GetCards(player, CardLocation.Deck).ForEach((c) =>
            {
                c.SetCode(0);
                c.AddAnimation(c.Swing(), 0.3f);
            });
        }

        private void OnConfirmCards(BinaryReader reader)
        {
            reader.ReadByte();
            int count = reader.ReadByte();
            List<ClientCard> cards = new List<ClientCard>();
            for (int i = 0; i < count; ++i)
            {
                uint code = reader.ReadUInt32();
                Coordinate coor = reader.ReadCoordinate();
                var card = field.GetCard(coor);
                card.SetCode(code);
                cards.Add(card);
            }
            if (cards.Count > 8)
            {
                field.SpreadConfirm(cards);
            }
            else
            {
                float duration = 0.5f + cards.Count * 0.25f;
                float left = -7 * (cards.Count - 1) * 0.5f;
                for (int i = 0; i < cards.Count; ++i)
                {
                    cards[i].AddAnimation(cards[i].ConfirmCloseup(left + 7 * i, duration), duration + 0.25f);
                }
            }
        }

        private void OnConfirmExtraTop(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            int count = reader.ReadByte();
            var excards = field.GetCards(player, CardLocation.Extra, true);
            int pcount = 0;
            for (int i = excards.Count - 1; i > 0; i--)
            {
                if ((excards[i].Position & (int)CardPosition.FaceUp) > 0)
                {
                    pcount++;
                }
            }
            List<ClientCard> topcards = new List<ClientCard>();
            for (int i = 0; i < count; ++i)
            {
                uint code = reader.ReadUInt32();
                reader.ReadBytes(3);
                excards[excards.Count - 1 - i - pcount].SetCode(code);
                topcards.Add(excards[excards.Count - 1 - i - pcount]);
            }
            for (int i = 0; i < topcards.Count; ++i)
            {
                topcards[i].AddAnimation(topcards[i].SingleConfirm(0.5f, 0.7f * i), 0.5f + 0.7f * i);
            }
        }

        private void OnConfirmDeckTop(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            int count = reader.ReadByte();
            var deck = field.GetCards(player, CardLocation.Deck, true);
            var topcards = new List<ClientCard>();
            for (int i = 0; i < count; ++i)
            {
                uint code = reader.ReadUInt32();
                reader.BaseStream.Position += 3;
                deck[deck.Count - 1 - i].SetCode(code);
                topcards.Add(deck[deck.Count - 1 - i]);
            }
            for (int i = 0; i < topcards.Count; ++i)
            {
                topcards[i].AddAnimation(topcards[i].SingleConfirm(0.5f, 0.7f * i), 0.5f + 0.7f * i);
            }
        }

        private void OnSortChain(BinaryReader reader)
        {
            if (Config.GetBool("AutoChain"))
            {
                MainGame.Instance.SendCToSResponse(-1);
                return;
            }
            reader.ReadByte();
            field.SelectMax = reader.ReadByte();
            field.SelectMin = 0;
            if (GameInfo.Instance.CurrentMessage == GameMessage.SortChain)
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(SysString(206));
            }
            else
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(SysString(205));
            }
            for (byte i = 0; i < field.SelectMax; ++i)
            {
                uint code = reader.ReadUInt32();
                var card = field.GetCard(reader.ReadCoordinate());
                if (code != 0)
                {
                    card.SetCode(code);
                }
                card.Selectable = true;
                card.Selected = false;
                card.SelectSequence = i;
                field.SelectableCards.Add(card);
            }
            field.SpreadSelection();
        }

        private void OnDisfield(BinaryReader reader)
        {
            reader.ReadByte();
            byte selmin = reader.ReadByte();
            uint selectalbe = ~reader.ReadUInt32();
            if (GameInfo.Instance.CurrentMessage == GameMessage.SelectPlace)
            {
                if (GameInfo.Instance.SelectHint > 0)
                {
                    MainGame.Instance.DuelWindow.ShowSelectHint(string.Format(SysString(569), GetName(GameInfo.Instance.SelectHint)));
                    GameInfo.Instance.SelectHint = 0;
                }
                else
                {
                    MainGame.Instance.DuelWindow.ShowSelectHint(SysString(560));
                }
            }
            else
            {
                if (GameInfo.Instance.SelectHint > 0)
                {
                    MainGame.Instance.DuelWindow.ShowSelectHint(GetName(GameInfo.Instance.SelectHint));
                    GameInfo.Instance.SelectHint = 0;
                }
                else
                {
                    MainGame.Instance.DuelWindow.ShowSelectHint(SysString(570));
                }
            }
            if (GameInfo.Instance.CurrentMessage == GameMessage.SelectPlace &&
                (Config.GetBool("AutoMPlace") && (selectalbe & 0x7f007f) > 0 || Config.GetBool("AutoSPlace") && (selectalbe & 0x7f007f) == 0))
            {
                uint filter;
                byte pzone = 0;
                MemoryStream stream = new MemoryStream();
                BinaryWriter response = new BinaryWriter(stream);
                if ((selectalbe & 0x7f) > 0)
                {
                    response.Write((byte)GameInfo.Instance.LocalPlayer(0));
                    response.Write((byte)CardLocation.MonsterZone);
                    filter = selectalbe & 0x7f;
                }
                else if ((selectalbe & 0x1f00) > 0)
                {
                    response.Write((byte)GameInfo.Instance.LocalPlayer(0));
                    response.Write((byte)CardLocation.SpellZone);
                    filter = (selectalbe >> 8) & 0x7f;
                }
                else if ((selectalbe & 0xc000) > 0)
                {
                    response.Write((byte)GameInfo.Instance.LocalPlayer(0));
                    response.Write((byte)CardLocation.SpellZone);
                    filter = (selectalbe >> 14) & 0x3;
                    pzone = 1;
                }
                else if ((selectalbe & 0x7f0000) > 0)
                {
                    response.Write((byte)GameInfo.Instance.LocalPlayer(1));
                    response.Write((byte)CardLocation.MonsterZone);
                    filter = (selectalbe >> 16) & 0x7f;
                }
                else if ((selectalbe & 0x1f000000) > 0)
                {
                    response.Write((byte)GameInfo.Instance.LocalPlayer(1));
                    response.Write((byte)CardLocation.SpellZone);
                    filter = (selectalbe >> 24) & 0x1f;
                }
                else
                {
                    response.Write((byte)GameInfo.Instance.LocalPlayer(1));
                    response.Write((byte)CardLocation.SpellZone);
                    filter = (selectalbe >> 30) & 0x3;
                    pzone = 1;
                }
                if (pzone == 0)
                {
                    if (Config.GetBool("RandomPlace"))
                    {
                        byte place;
                        do
                        {
                            place = (byte)Random.Range(0, 7);
                        } while ((filter & (1 << place)) == 0);
                        response.Write(place);
                    }
                    else
                    {
                        if ((filter & 0x40) > 0)
                        {
                            response.Write((byte)6);
                        }
                        else if ((filter & 0x20) > 0)
                        {
                            response.Write((byte)5);
                        }
                        else if ((filter & 0x4) > 0)
                        {
                            response.Write((byte)2);
                        }
                        else if ((filter & 0x2) > 0)
                        {
                            response.Write((byte)1);
                        }
                        else if ((filter & 0x8) > 0)
                        {
                            response.Write((byte)3);
                        }
                        else if ((filter & 0x1) > 0)
                        {
                            response.Write((byte)0);
                        }
                        else if ((filter & 0x10) > 0)
                        {
                            response.Write((byte)4);
                        }
                    }
                }
                else
                {
                    if ((filter & 0x1) > 0)
                    {
                        response.Write((byte)6);
                    }
                    else if ((filter & 0x2) > 0)
                    {
                        response.Write((byte)7);
                    }
                }
                MainGame.Instance.SendCToSResponse(stream.ToArray());
            }
            else
            {
                uint filter = 0x1;
                field.SelectMin = selmin;
                for (byte i = 0; i < 7; ++i, filter <<= 1)
                {
                    if ((selectalbe & filter) > 0)
                    {
                        PlaceIndicator.Create(0, CardLocation.MonsterZone, i);
                    }
                }
                filter = 0x100;
                for (int i = 0; i < 8; ++i, filter <<= 1)
                {
                    if ((selectalbe & filter) > 0)
                    {
                        if (i == 6)
                        {
                            PlaceIndicator.Create(0, CardLocation.SpellZone, 0);
                        }
                        else if (i == 7)
                        {
                            PlaceIndicator.Create(0, CardLocation.SpellZone, 4);
                        }
                        else
                        {
                            PlaceIndicator.Create(0, CardLocation.SpellZone, i);
                        }
                    }
                }
                filter = 0x10000;
                for (int i = 0; i < 7; ++i, filter <<= 1)
                {
                    if ((selectalbe & filter) > 0)
                    {
                        PlaceIndicator.Create(1, CardLocation.MonsterZone, i);
                    }
                }
                filter = 0x1000000;
                for (int i = 0; i < 8; ++i, filter <<= 1)
                {
                    if ((selectalbe & filter) > 0)
                    {
                        if (i == 6)
                        {
                            PlaceIndicator.Create(1, CardLocation.SpellZone, 0);
                        }
                        else if (i == 7)
                        {
                            PlaceIndicator.Create(1, CardLocation.SpellZone, 4);
                        }
                        else
                        {
                            PlaceIndicator.Create(1, CardLocation.SpellZone, i);
                        }
                    }
                }
                PlaceIndicator.CheckCount();
            }
        }

        private void OnCardHint(BinaryReader reader)
        {
            Coordinate coor = reader.ReadCoordinate();
            reader.ReadByte();
            CardHint hint = (CardHint)reader.ReadByte();
            uint val = reader.ReadUInt32();
            var card = field.GetCard(coor);
            card.SetCardHints(hint, val);
        }

        private void OnSwap(BinaryReader reader)
        {
            reader.ReadInt32();
            Coordinate c1 = reader.ReadCoordinate();
            reader.ReadByte();
            reader.ReadInt32();
            Coordinate c2 = reader.ReadCoordinate();
            reader.ReadByte();
            GameInfo.Instance.EventTiming = SysString(1602);
            var card1 = field.GetCard(c1);
            var card2 = field.GetCard(c2);
            card1.SetState(c2);
            card2.SetState(c1);
            field.MoveCard(card1, 0.1f, 0.1f);
            field.MoveCard(card2, 0.1f, 0.1f);
        }

        private void NotImplemented(BinaryReader reader)
        {
        }

        private void OnPosChange(BinaryReader reader)
        {
            uint code = reader.ReadUInt32();
            Coordinate coor = reader.ReadCoordinate();
            int prePos = reader.ReadByte();
            int curPos = reader.ReadByte();
            var card = field.GetCard(coor);
            if ((prePos & (int)CardPosition.FaceUp) > 0 && (curPos & (int)CardPosition.FaceDown) > 0)
            {
                card.Counters.Clear();
            }
            card.SetCode(code);
            card.Position = curPos;
            field.MoveCard(card, 0.2f, 0.1f);
            GameInfo.Instance.EventTiming = SysString(1600);
        }

        private void OnReverseDeck(BinaryReader reader)
        {
            GameInfo.Instance.IsDeckReversed = !GameInfo.Instance.IsDeckReversed;
            var list = field.GetCards(0, CardLocation.Deck);
            list.AddRange(field.GetCards(1, CardLocation.Deck));
            list.ForEach((c) => field.MoveCard(c, 0.2f, 0.1f));
        }

        private void OnDeckTop(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            int seq = reader.ReadByte();
            uint code = reader.ReadUInt32();
            var deck = field.GetCards(player, CardLocation.Deck, true);
            var card = deck[deck.Count - 1 - seq];
            card.SetCode(code);
            bool rev = (code & 0x80000000) != 0;
        }

        private void OnNewPhase(BinaryReader reader)
        {
            int phase = reader.ReadInt16();
            MainGame.Instance.DuelWindow.NewPhase(phase);
        }

        private void OnNewTurn(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            MainGame.Instance.DuelWindow.NewTurn(player);
        }

        private void OnHsWatchChange(BinaryReader reader)
        {
            MainGame.Instance.RoomWindow.UpdateWatcherCount(reader.ReadUInt16());
        }

        private void OnTimeLimit(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            reader.ReadByte();
            int time = reader.ReadUInt16();
            if (player == 0)
            {
                NetworkService.Instance.Send(CToSMessage.TimeConfirm);
            }
            //todo update time
        }

        private void OnDuelStart(BinaryReader r = null)
        {
            GameInfo.Instance.IsDeckReversed = false;
            GameInfo.Instance.Turn = 0;
            GameInfo.Instance.TimeLeft[0] = 0;
            GameInfo.Instance.TimeLeft[1] = 0;
            GameInfo.Instance.IsReplaySwapped = false;
            GameInfo.Instance.MatchKill = 0;
            if (MainGame.Instance.DeckBuilder.IsSideChanging)
            {
                MainGame.Instance.DeckBuilder.Hide();
            }
            if (!GameInfo.Instance.IsTag && GameInfo.Instance.SelfType > 1 || GameInfo.Instance.IsTag && GameInfo.Instance.SelfType > 3)
            {
                GameInfo.Instance.PlayerType = PlayerType.Observer;
            }
            MainGame.Instance.RoomWindow.Hide();
            MainGame.Instance.RoomWindow.ExtractNames();
            GameInfo.Instance.TagPlayer[0] = false;
            GameInfo.Instance.TagPlayer[1] = false;
            MainGame.Instance.DuelWindow.Show();
            field.Show();
            MainGame.Instance.CameraToDuel();
        }

        private void OnTypeChange(BinaryReader reader)
        {
            byte type = reader.ReadByte();
            GameInfo.Instance.SelfType = (byte)(type & 0xf);
            GameInfo.Instance.IsHost = ((type >> 4) & 0xf) != 0;
            MainGame.Instance.RoomWindow.ChangeType();
        }

        private void OnSToCHandResult(BinaryReader reader)
        {
            Debug.Log("result1:" + reader.ReadByte().ToString() + "  result2:" + reader.ReadByte().ToString());
        }

        private void OnSelectTp(BinaryReader r = null)
        {
            MainGame.Instance.OptionSelector.SelectTp();
        }

        private void OnSelectHand(BinaryReader r = null)
        {
            MainGame.Instance.RoomWindow.Hide();
            MainGame.Instance.ImageSelector.SelectHand();
        }

        private void OnPlayerChange(BinaryReader reader)
        {
            byte status = reader.ReadByte();
            int position = (status >> 4) & 0xf;
            int state = status & 0xf;
            MainGame.Instance.RoomWindow.PlayerChange(position, state);
        }

        private void OnError(BinaryReader reader)
        {
            ErrorType error = (ErrorType)reader.ReadByte();
            Debug.Log(error.ToString());
            reader.ReadBytes(3);
            uint code = reader.ReadUInt32();
            string errorstr = null;
            switch (error)
            {
                case ErrorType.JoinError:
                    if (code == 0 || code == 1 || code == 2)
                    {
                        errorstr = (SysString(1403 + code));
                    }
                    break;
                case ErrorType.DeckError:
                    MainGame.Instance.RoomWindow.SetDeckListEnable(true);
                    uint cardcode = code & 0xFFFFFFF;
                    uint flag = code >> 28;
                    switch (flag)
                    {
                        case (uint)DeckError.LfList:
                            errorstr = string.Format(SysString(1407), GetCardData(cardcode).Name);
                            break;
                        case (uint)DeckError.OcgOnly:
                            errorstr = string.Format(SysString(1413), GetCardData(cardcode).Name);
                            break;
                        case (uint)DeckError.TcgOnly:
                            errorstr = string.Format(SysString(1414), GetCardData(cardcode).Name);
                            break;
                        case (uint)DeckError.UnknownCard:
                            errorstr = string.Format(SysString(1415), GetCardData(cardcode).Name, cardcode);
                            break;
                        case (uint)DeckError.CardCount:
                            errorstr = string.Format(SysString(1416), GetCardData(cardcode).Name, cardcode);
                            break;
                        case (uint)DeckError.MainCount:
                            errorstr = string.Format(SysString(1417), cardcode.ToString());
                            break;
                        case (uint)DeckError.ExtraCount:
                            if (code > 0)
                            {
                                errorstr = string.Format(SysString(1418), cardcode.ToString());
                            }
                            else
                            {
                                errorstr = SysString(1420);
                            }
                            break;
                        case (uint)DeckError.SideCount:
                            errorstr = string.Format(SysString(1419), cardcode.ToString());
                            break;
                        default:
                            errorstr = SysString(1406);
                            break;
                    }
                    break;
                case ErrorType.SideError:
                    errorstr = SysString(1408);
                    break;
                case ErrorType.VerionError:
                    errorstr = SysString(1411).
                        Replace("%X.0%X.%X", (code >> 12).ToString() + ((code >> 4) & 0xff).ToString() + (code & 0xf).ToString());
                    break;
                default:
                    errorstr = "unknown error";
                    break;
            }
            MainGame.Instance.HintBox.ShowHint(errorstr, 0.5f);
        }

        private void OnChat(BinaryReader reader)
        {
            int player = reader.ReadUInt16();
            string ustr = reader.ReadUnicode(512);
            Debug.Log(string.Format("-->{0} chat: {1}", player.ToString(), ustr));
            if (player < 4)
            {
                if (Config.GetBool("IgnoreChating", false))
                {
                    return;
                }
                if (!GameInfo.Instance.IsTag)
                {
                    if (MainGame.Instance.DuelWindow.IsShowing)
                    {
                        player = GameInfo.Instance.LocalPlayer(player);
                    }
                }
                else
                {
                    if (MainGame.Instance.DuelWindow.IsShowing && !GameInfo.Instance.IsFirst)
                    {
                        player ^= 2;
                    }
                    if (player == 0)
                    {
                        player = 0;
                    }
                    else if (player == 1)
                    {
                        player = 2;
                    }
                    else if (player == 2)
                    {
                        player = 1;
                    }
                    else if (player == 3)
                    {
                        player = 3;
                    }
                    else
                    {
                        player = 10;
                    }
                }
            }
            else
            {
                if (player == 8)
                {
                    if (Config.GetBool("IgnoreChating", false))
                    {
                        return;
                    }
                }
                else if (player < 11 || player > 19)
                {
                    //todo: if (mainGame->chkIgnore2->isChecked())
                    player = 10;
                }
            }
            MainGame.Instance.ChatWindow.AddMessage(player, ustr);
        }

        private void OnJoinGame(BinaryReader reader)
        {
            uint lfList = reader.ReadUInt32();
            byte rule = reader.ReadByte();
            byte mode = reader.ReadByte();
            byte duelRule = reader.ReadByte();
            bool doNotCheck = reader.ReadByte() > 0;
            bool doNotSuffle = reader.ReadByte() > 0;
            reader.ReadBytes(3);
            uint startLp = reader.ReadUInt32();
            byte startHand = reader.ReadByte();
            byte drawCount = reader.ReadByte();
            ushort timeLimit = reader.ReadUInt16();

            StringBuilder builder = new StringBuilder();
            int flidx = DeckService.ForbiddenLists.FindIndex((fl) => fl.Hash == lfList);
            if (flidx >= 0)
            {
                var flist = DeckService.ForbiddenLists[flidx];
                MainGame.Instance.DeckBuilder.CurrentFList = flist;
                builder.Append(SysString(1226)).Append(flist.Name);
            }
            else
            {
                builder.Append(SysString(1226)).Append("unknown");
            }
            builder.Append(SysString(1225)).Append(SysString((uint)1240 + rule)).Append("\n");
            builder.Append(SysString(1227)).Append(SysString((uint)1244 + mode)).Append("\n");
            if (timeLimit > 0)
            {
                builder.Append(SysString(1237)).Append(timeLimit.ToString()).Append("\n");
            }
            builder.Append(SysString(1231)).Append(startLp.ToString()).Append("\n");
            builder.Append(SysString(1232)).Append(startHand.ToString()).Append("\n");
            builder.Append(SysString(1233)).Append(drawCount.ToString()).Append("\n");
            if (duelRule != Config.DefaultMasterRule)
            {
                builder.Append(SysString((uint)1260 + duelRule - 1)).Append("\n");
            }
            if (doNotCheck)
            {
                builder.Append("<b>").Append(SysString(1229)).Append("</b>").Append("\n");
            }
            if (doNotSuffle)
            {
                builder.Append("<b>").Append(SysString(1230)).Append("</b>").Append("\n");
            }
            GameInfo.Instance.IsTag = mode == 2;
            GameInfo.Instance.TimeLimit = timeLimit;
            GameInfo.Instance.TimeLeft[0] = 0;
            GameInfo.Instance.TimeLeft[1] = 0;
            GameInfo.Instance.DuelRule = duelRule;
            GameInfo.Instance.WatchingCount = 0;
            MainGame.Instance.ServerWindow.Hide();
            MainGame.Instance.RoomWindow.Show(builder.ToString());
        }

        private void OnPlayerEnter(BinaryReader reader)
        {
            string name = reader.ReadUnicode(40);
            byte pos = reader.ReadByte();
            MainGame.Instance.RoomWindow.PlayerEnter(pos, name);
        }

        private void OnSelectBattleCmd(BinaryReader reader)
        {
            reader.ReadByte();
            int code, desc, count, flag = 0;
            count = reader.ReadByte();
            for (int i = 0; i < count; ++i)
            {
                code = reader.ReadInt32();
                Coordinate coord = reader.ReadCoordinate();
                desc = reader.ReadInt32();
                var card = field.GetCard(coord);
                if ((code & 0x80000000) > 0)
                {
                    code &= 0x7fffffff;
                    flag = (int)EDEsc.Operation;
                }
                card.AddActivateCommand(i, desc, flag);
                field.InteractiveCards.Add(card);
            }
            count = reader.ReadByte();
            for (int i = 0; i < count; ++i)
            {
                reader.ReadInt32();
                var card = field.GetCard(reader.ReadCoordinate());
                reader.ReadByte();
                card.AddCommand(CardCommand.Attack, i);
                field.InteractiveCards.Add(card);
            }
            MainGame.Instance.DuelWindow.WhenSelectBattleCmd(reader.ReadByte() > 0, reader.ReadByte() > 0);
        }

        private void OnSelectChain(BinaryReader reader)
        {
            reader.ReadByte();
            int count = reader.ReadByte();
            int specount = reader.ReadByte();
            IsChainForced = reader.ReadBoolean();
            reader.ReadInt64();
            bool select_trigger = (specount == 0x7f);
            Debug.Log(string.Format("-->specount: {0}, forced: {1}, select_trigger {2}",
                specount.ToString("X").PadLeft(2, '0'), IsChainForced, select_trigger));
            for (int i = 0; i < count; ++i)
            {
                int flag = reader.ReadByte();
                uint code = reader.ReadUInt32();
                Coordinate curCoord = reader.ReadCoordinate();
                int subSeq = reader.ReadByte();
                int desc = reader.ReadInt32();
                var card = field.GetCard(curCoord, subSeq);
                card.SetCode(code);
                field.InteractiveCards.Add(card);
                card.AddActivateCommand(i, desc, flag);
            }
            if (!MainGame.Instance.DuelWindow.TglChainAlways.isOn && !select_trigger && !IsChainForced)
            {
                if (count == 0 || MainGame.Instance.DuelWindow.TglChainIngnore.isOn || specount == 0)
                {
                    MainGame.Instance.SendCToSResponse(-1);
                    MainGame.Instance.DuelWindow.SetCancelOrFinish(0);
                    return;
                }
            }
            if (MainGame.Instance.DuelWindow.TglChainIngnore.isOn && !IsChainForced)
            {
                MainGame.Instance.SendCToSResponse(-1);
                MainGame.Instance.DuelWindow.SetCancelOrFinish(0);
                return;
            }
            if (!IsChainForced)
            {
                MainGame.Instance.DuelWindow.SetCancelOrFinish(1);
            }
            MainGame.Instance.DuelWindow.ShowSelectHint(GameInfo.Instance.EventTiming + "\n" + SysString(550));
        }

        private void OnSelectCard(BinaryReader reader)
        {
            field.EndQuery();
            reader.ReadByte();
            field.Cancelabel = reader.ReadByte() != 0;
            field.SelectMin = reader.ReadByte();
            field.SelectMax = reader.ReadByte();
            int count = reader.ReadByte();
            bool needSpread = false;

            Debug.Log(string.Format("-->select: min={0}, max={1}", field.SelectMin, field.SelectMax));
            if (GameInfo.Instance.SelectHint > 0)
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(string.Format("{0}({1}-{2})",
                    GetDescription(GameInfo.Instance.SelectHint), field.SelectMin, field.SelectMax));
                GameInfo.Instance.SelectHint = 0;
            }
            else
            {
                MainGame.Instance.DuelWindow.ShowSelectHint(string.Format("{0}({1}-{2})",
                    SysString(560), field.SelectMin, field.SelectMax));
            }

            for (byte i = 0; i < count; ++i)
            {
                uint code = reader.ReadUInt32();
                var card = field.GetCard(reader.ReadCoordinate(), reader.ReadByte());
                if (code != 0)
                {
                    card.SetCode(code);
                }
                field.SelectableCards.Add(card);
                Debug.Log(string.Format("-->{0} is selectable", GetName(card.Data.Code)));
                card.SelectSequence = i;
                card.Selectable = true;
                card.Selected = false;
                if ((card.Location & 0xf1) > 0)
                {
                    needSpread = true;
                }
            }
            if (Config.GetBool("AutoSelection", true)
                && (!field.Cancelabel && (count == 1 || count == field.SelectMin && count == field.SelectMax)))
            {
                field.SelectedCards.AddRange(field.SelectableCards);
                field.SendSelectResponse();
                return;
            }
            if (field.Cancelabel)
            {
                MainGame.Instance.DuelWindow.SetCancelOrFinish(1);
            }
            if (needSpread)
            {
                field.SpreadSelection();
            }
        }

        private void OnSelectOption(BinaryReader reader)
        {
            reader.ReadByte();
            int count = reader.ReadByte();
            if (Config.GetBool("AutoSelection", false) && count == 1)
            {
                MainGame.Instance.HintBox.ShowHint(TzlString(1) + GetDescription(reader.ReadUInt32()), 0.5f);
                MainGame.Instance.SendCToSResponse(0);
                return;
            }
            List<Tuple<int, int>> opt = new List<Tuple<int, int>>();
            for (int i = 0; i < count; ++i)
            {
                opt.Add(new Tuple<int, int>(i, reader.ReadInt32()));
            }
            MainGame.Instance.OptionSelector.SelectOptions(opt);
        }

        private void OnSelectYesNo(BinaryReader reader)
        {
            reader.ReadByte();
            MainGame.Instance.ConfirmWindow.SelectYesNo(GetDescription(reader.ReadUInt32()));
        }

        private void OnSelectEffectYN(BinaryReader reader)
        {
            reader.ReadByte();
            uint code = reader.ReadUInt32();
            var card = field.GetCard(reader.ReadCoordinate());
            reader.ReadByte();
            card.SetCode(code);
            if (card.Location != (int)CardLocation.Deck)
            {
                card.HighLight();
            }
            uint desc = reader.ReadUInt32();
            string title;
            if (desc == 0)
            {
                title = string.Format(SysString(200), FormatLocation(card.Location, card.Sequence),
                   GetName(card.Data.Code));
            }
            else if (desc == 221)
            {
                title = string.Format(SysString(221), FormatLocation(card.Location, card.Sequence),
                     GetName(card.Data.Code)) + "\n" + SysString(223);
            }
            else if (desc < 2048)
            {
                title = string.Format(SysString(desc), GetName(card.Data.Code));
            }
            else
            {
                title = string.Format(GetDescription(desc), GetName(card.Data.Code));
            }
            MainGame.Instance.ConfirmWindow.SelectYesNo(title.ToString());
        }

        private void OnSelectIdleCmd(BinaryReader reader)
        {
            reader.ReadByte();
            int code, count;
            count = reader.ReadByte();
            for (int i = 0; i < count; ++i)
            {
                _ = reader.ReadInt32();
                var coord = reader.ReadCoordinate();
                var card = field.GetCard(coord);
                card.AddCommand(CardCommand.Summon, i);
                field.InteractiveCards.Add(card);
            }
            count = reader.ReadByte();
            for (int i = 0; i < count; ++i)
            {
                code = reader.ReadInt32();
                var card = field.GetCard(reader.ReadCoordinate());
                card.AddCommand(CardCommand.SpSummon, i);
                field.InteractiveCards.Add(card);
            }
            count = reader.ReadByte();
            for (int i = 0; i < count; ++i)
            {
                code = reader.ReadInt32();
                var card = field.GetCard(reader.ReadCoordinate());
                card.AddCommand(CardCommand.Repos, i);
                field.InteractiveCards.Add(card);
            }
            count = reader.ReadByte();
            for (int i = 0; i < count; ++i)
            {
                code = reader.ReadInt32();
                var card = field.GetCard(reader.ReadCoordinate());
                card.AddCommand(CardCommand.MSet, i);
                field.InteractiveCards.Add(card);
            }
            count = reader.ReadByte();
            for (int i = 0; i < count; ++i)
            {
                code = reader.ReadInt32();
                var card = field.GetCard(reader.ReadCoordinate());
                card.AddCommand(CardCommand.SSet, i);
                field.InteractiveCards.Add(card);
            }
            count = reader.ReadByte();
            for (int i = 0; i < count; ++i)
            {
                code = reader.ReadInt32();
                var card = field.GetCard(reader.ReadCoordinate());
                int desc = reader.ReadInt32();
                int flag = 0;
                if ((code & 0x80000000) > 0)
                {
                    code &= 0x7fffffff;
                    flag = 1;
                }
                card.AddActivateCommand(i, desc, flag);
                field.InteractiveCards.Add(card);
            }
            MainGame.Instance.DuelWindow.WhenSelectIdleCmd(reader.ReadByte() > 0, reader.ReadByte() > 0, reader.ReadByte() > 0);
        }

        private void OnStart(BinaryReader reader)
        {
            int playertype = reader.ReadByte();
            GameInfo.Instance.DuelRule = reader.ReadByte();
            GameInfo.Instance.Lp[0] = reader.ReadInt32();
            GameInfo.Instance.Lp[1] = reader.ReadInt32();
            GameInfo.Instance.IsFirst = !((playertype & 0xf) > 0);
            if ((playertype & 0xf0) > 0)
            {
                GameInfo.Instance.PlayerType = PlayerType.Observer;
            }
            if (GameInfo.Instance.IsTag)
            {
                if (GameInfo.Instance.IsFirst)
                {
                    GameInfo.Instance.TagPlayer[1] = true;
                }
                else
                {
                    GameInfo.Instance.TagPlayer[0] = true;
                }
            }
            GameInfo.Instance.Turn = 0;

            int deckc = reader.ReadInt16();
            int extrac = reader.ReadInt16();
            int deckc2 = reader.ReadInt16();
            int extrac2 = reader.ReadInt16();
            MainGame.Instance.DuelWindow.FirstAwake();
            MainGame.Instance.Descriptor.Show();
            field.InitField(GameInfo.Instance.LocalPlayer(0), deckc, extrac);
            field.InitField(GameInfo.Instance.LocalPlayer(1), deckc2, extrac2);
        }

        private void OnUpdateCard(BinaryReader reader)
        {
            ClientCard card = field.GetCard(reader.ReadCoordinate());
            reader.ReadInt32();
            card.UpdateInfo(reader);
        }

        private void OnUpdateData(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            int location = reader.ReadByte();
            field.UpdateFieldCard(player, location, reader);
        }

        private void OnWainting(BinaryReader reader)
        {
        }

        private void OnWin(BinaryReader reader)
        {
            MainGame.Instance.CurrentWindow.Hide();
            int player = reader.ReadByte();
            uint wincode = reader.ReadByte();
            if (player == 2)
            {
                //todo
            }
            else if (GameInfo.Instance.LocalPlayer(player) == 0)
            {
                if (GameInfo.Instance.MatchKill > 0)
                {
                    MainGame.Instance.HintBox.ShowHint(
                        string.Format(VictoryString(0x20), GetName((uint)GameInfo.Instance.MatchKill)), 0.5f);
                }
                else if (wincode < 0x10)
                {
                    MainGame.Instance.HintBox.ShowHint(
                        string.Format("[{0}]{1}", GameInfo.Instance.ClientName, VictoryString(wincode)), 1);
                }
                else
                {
                    MainGame.Instance.HintBox.ShowHint(VictoryString(wincode), 1);
                }
            }
            else
            {
                if (GameInfo.Instance.MatchKill > 0)
                {
                    MainGame.Instance.HintBox.ShowHint(
                        string.Format(VictoryString(0x20), GetName((uint)GameInfo.Instance.MatchKill)), 1);
                }
                else if (wincode < 0x10)
                {
                    MainGame.Instance.HintBox.ShowHint(
                        string.Format("[{0}]{1}", GameInfo.Instance.HostName, VictoryString(wincode)), 0.5f);
                }
                else
                {
                    MainGame.Instance.HintBox.ShowHint(VictoryString(wincode), 1);
                }
            }
        }

        private void OnHint(BinaryReader reader)
        {
            GameHint hint = (GameHint)reader.ReadByte();
            reader.ReadByte();
            uint data = reader.ReadUInt32();
            Debug.Log("-->hint type:" + hint.ToString());
            switch (hint)
            {
                case GameHint.Event:
                    GameInfo.Instance.EventTiming = GetDescription(data);
                    break;
                case GameHint.Message:
                    MainGame.Instance.HintBox.ShowHint(GetDescription(data), 1);
                    break;
                case GameHint.SelectMessage:
                    GameInfo.Instance.SelectHint = data;
                    break;
                case GameHint.OpSelected:
                    MainGame.Instance.HintBox.ShowHint(string.Format(SysString(1510), GetDescription(data)), 0.65f);
                    break;
                case GameHint.Effect:
                    MainGame.Instance.DuelWindow.ChainingCloseup(data, 0.5f);
                    break;
                case GameHint.Race:
                    MainGame.Instance.HintBox.ShowHint(string.Format(SysString(1511), FormatRace(data)), 0.65f);
                    break;
                case GameHint.Attribute:
                    MainGame.Instance.HintBox.ShowHint(string.Format(SysString(1511), FormatAttribute(data)), 0.65f);
                    break;
                case GameHint.Code:
                    MainGame.Instance.HintBox.ShowHint(string.Format(SysString(1511), GetName(data)), 0.65f);
                    break;
                case GameHint.Number:
                    MainGame.Instance.HintBox.ShowHint(string.Format(SysString(1512), data.ToString()), 0.65f);
                    break;
                case GameHint.Card:
                    MainGame.Instance.DuelWindow.ChainingCloseup(data);
                    break;
                default: break;
            }
        }

        private void OnRetry(BinaryReader reader)
        {
            if (NetworkService.Instance.Connected)
            {
                NetworkService.Instance.Disconnect();
            }

            MainGame.Instance.HintBox.ShowHint("error occured", 1);
            MainGame.Instance.DuelWindow.Hide();
            field.Clear();
            isWorking = false;
            MainGame.Instance.ServerWindow.Show();
            MainGame.Instance.ChatWindow.Hide();
        }

        private void OnReloadField(BinaryReader reader)
        {
            GameInfo.Instance.DuelRule = reader.ReadByte();
            int val;
            for (int i = 0; i < 2; ++i)
            {
                int p = GameInfo.Instance.LocalPlayer(i);
                GameInfo.Instance.Lp[p] = reader.ReadInt32();
                for (int seq = 0; seq < 7; ++seq)
                {
                    val = reader.ReadByte();
                    if (val > 0)
                    {
                        var card = field.AddNewCard(p, (int)CardLocation.MonsterZone, seq, reader.ReadByte());
                        val = reader.ReadByte();
                        if (val > 0)
                        {
                            for (int c = 0; c < val; ++c)
                            {
                                var xyz = field.AddNewCard(p, (int)CardLocation.MonsterZone, 0);
                                card.Overlay.Add(xyz);
                                xyz.OverlayTarget = card;
                                xyz.Sequence = card.Overlay.Count - 1;
                                xyz.Location = (int)CardLocation.Overlay;
                            }
                        }
                    }
                }
                for (int seq = 0; seq < 8; ++seq)
                {
                    val = reader.ReadByte();
                    if (val > 0)
                    {
                        field.AddNewCard(p, (int)CardLocation.SpellZone, seq, reader.ReadByte());
                    }
                }
                val = reader.ReadByte();
                for (int seq = 0; seq < val; ++seq)
                {
                    field.AddNewCard(p, (int)CardLocation.Deck, seq);
                }
                val = reader.ReadByte();
                for (int seq = 0; seq < val; ++seq)
                {
                    field.AddNewCard(p, (int)CardLocation.Hand, seq);
                }
                val = reader.ReadByte();
                for (int seq = 0; seq < val; ++seq)
                {
                    field.AddNewCard(p, (int)CardLocation.Grave, seq, 0x1);
                }
                val = reader.ReadByte();
                for (int seq = 0; seq < val; ++seq)
                {
                    field.AddNewCard(p, (int)CardLocation.Removed, seq);
                }
                val = reader.ReadByte();
                for (int seq = 0; seq < val; ++seq)
                {
                    field.AddNewCard(p, (int)CardLocation.Extra, seq);
                }
                val = reader.ReadByte();
            }
            val = reader.ReadByte();
            for (int i = 0; i < val; ++i)
            {
                uint code = reader.ReadUInt32();
                var pCoord = reader.ReadCoordinate();
                int pss = reader.ReadByte();
                var cCoord = reader.ReadCoordinate();
                uint desc = reader.ReadUInt32();
                var card = field.GetCard(pCoord, pss);
                card.SetCode(code);
                field.BeginChaining(cCoord, card);
            }
            field.RefreshAll();
        }

        private void OnPlayerHint(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            int hint = reader.ReadByte();
            int value = reader.ReadInt32();
            //todo
        }

        private void OnDraw(BinaryReader reader)
        {
            int player = GameInfo.Instance.LocalPlayer(reader.ReadByte());
            int count = reader.ReadByte();
            var deck = field.GetCards(player, CardLocation.Deck, true);
            for (int i = 0; i < count; ++i)
            {
                var card = deck[deck.Count - 1 - i];
                uint code = reader.ReadUInt32();
                if (!GameInfo.Instance.IsDeckReversed || code > 0)
                {
                    card.SetCode(code & 0x7fffffff);
                }
                card.Location = (int)CardLocation.Hand;
                card.Sequence = field.HandCards[player].Count;
                field.HandCards[player].Add(card);
            }
            field.ArrangeHand(player, 0.1f);
            GameInfo.Instance.EventTiming = string.Format(SysString(1611 + (uint)player), count.ToString());
        }
    }
}
