using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using YGOTzolkin.Service;
using YGOTzolkin.Utility;
using YGOTzolkin.YGOModel;
using Random = UnityEngine.Random;

namespace YGOTzolkin.Elements
{
    class ClientCard : IPoolSupporter
    {
        private static readonly ObjectPool<CommandButton> ButtonPool = new ObjectPool<CommandButton>();
        private static readonly ObjectPool<MonsterPanel> PanelPool = new ObjectPool<MonsterPanel>();

        internal int Position { get; set; }
        internal int Controller { get; set; }
        internal int Location { get; set; }
        internal int Sequence { get; set; }
        internal CardData Data { get; private set; }
        internal Dictionary<uint, int> DescriptionHints { get; private set; }
        internal Dictionary<CardHint, uint> CardHints { get; private set; }
        internal List<ClientCard> Overlay { get; private set; }
        internal ClientCard OverlayTarget { get; set; }
        internal Dictionary<uint, int> Counters { get; private set; }

        internal Transform Transform { get; private set; }
        internal TextMeshPro GroupText { get; private set; }
        internal Vector3 RealPosition { get; set; }
        internal Quaternion RealRotation { get; set; }
        internal Vector3 QueryingPosition { get; set; }

        private readonly GameObject faceObject;
        private readonly Transform decorator;

        public byte SelectSequence { get; set; }
        public bool Selectable { get; set; }
        public bool Selected { get; set; }
        public int SelectionParam { get; set; }

        private string levelStr;
     
        private readonly List<CommandButton> buttons;
        private CommandButton activateButton;
        private bool floating;
        private readonly DuelAnimation animation;
        //private AttackIndicator attackIndicator;
        private MonsterPanel infoPanel;

        public static int SequenceCompare(ClientCard bigger, ClientCard smaller)
        {
            return bigger.Sequence > smaller.Sequence ? 1 : -1;
        }

        public static int QueryCompare(ClientCard left, ClientCard right)
        {
            if (left.buttons.Count != right.buttons.Count)
            {
                return left.buttons.Count > right.buttons.Count ? -1 : 1;
            }
            return left.Sequence > right.Sequence ? -1 : 1;
        }

        public static int SelectionCompare(ClientCard left, ClientCard right)
        {
            //todo
            return 0;
        }

        public static int ActivateCompare(ClientCard left, ClientCard right)
        {
            if (left.Location == right.Location)
            {
                return left.Sequence > right.Sequence ? -1 : 1;
            }
            return left.Location > right.Location ? -1 : 1;
        }

        public ClientCard()
        {
            buttons = new List<CommandButton>();
            Data = new CardData();
            Overlay = new List<ClientCard>();
            floating = false;
            animation = new DuelAnimation();
            DescriptionHints = new Dictionary<uint, int>();
            CardHints = new Dictionary<CardHint, uint>();
            Counters = new Dictionary<uint, int>();

            Transform = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/ClientCard") as GameObject).transform;
            faceObject = Transform.GetChild(0).gameObject;
            decorator = Transform.GetChild(3);
            decorator.localScale = Vector3.zero;
            Transform.GetChild(4).gameObject.SetActive(false);

            GroupText = Transform.GetChild(2).GetComponent<TextMeshPro>();
            GroupText.transform.localScale = Vector3.zero;

            Tools.BindEvent(faceObject, EventTriggerType.PointerClick, OnClick);
            Tools.BindEvent(faceObject, EventTriggerType.PointerEnter, OnPointerEnter);
            Tools.BindEvent(faceObject, EventTriggerType.Scroll, MainGame.Instance.Descriptor.OnTextScroll);
            Tools.BindEvent(faceObject, EventTriggerType.PointerExit, OnPointerExit);
        }

        internal void SetState(int controller, int location, int sequence)
        {
            Controller = controller;
            Location = location;
            Sequence = sequence;
        }

        internal void SetState(Coordinate coord)
        {
            Controller = coord.Controller;
            Location = coord.Location;
            Sequence = coord.Sequence;
        }

        public void Initialize(int controller, int location, int sequence)
        {
            Controller = controller;
            Location = location;
            Sequence = sequence;
            SetCode(0);
            Visualize();
        }

        internal void Visualize()
        {
            Transform.GetChild(1).GetComponent<Renderer>().material.mainTexture =
               Controller == 0 ? TextureService.Cover : TextureService.Cover2;
            Transform.localScale = Vector3.one;
        }

        public void SetCode(uint code)
        {
            Data.Code = code;
            if (code == 0)
            {
                faceObject.GetComponent<Renderer>().material.mainTexture =
                    Controller == 0 ? TextureService.Cover : TextureService.Cover2;
            }
            else
            {
                faceObject.GetComponent<Renderer>().material.mainTexture = TextureService.GetCardTexture(Data.Code);
            }
        }

        public void Disable()
        {
            MainGame.Instance.FrameActions.Remove(Update);
            Transform.localScale = Vector3.zero;
            Location = -1;
            Position = 0;
            Controller = -1;
            if (buttons.Count > 0)
            {
                ButtonPool.Store(buttons);
                buttons.Clear();
            }
            if (infoPanel != null)
            {
                PanelPool.Store(infoPanel);
                infoPanel = null;
            }
            GroupText.gameObject.transform.localScale = Vector3.zero;
        }

        public void Reset()
        {
            Controller = 7;
            Location = 0;
            Sequence = 0;
            Data.Reset();
            Position = (int)CardPosition.FaceDownAttack;
            Counters.Clear();
            Transform.position = Vector3.zero;
            MainGame.Instance.FrameActions.Add(Update);
        }

        public void AddCommand(CardCommand command, int index)
        {
            Debug.Log(string.Format("[{0}] wants to add {1}, index={2}", DataService.GetName(Data.Code), command.ToString(), index));
            var button = ButtonPool.New();
            int response = 0;
            string text;
            switch (command)
            {
                case CardCommand.Summon:
                    text = DataService.SysString(1151);
                    response = index << 16;
                    break;
                case CardCommand.SpSummon:
                    text = DataService.SysString(1152);
                    response = (index << 16) + 1;
                    break;
                case CardCommand.MSet:
                    text = DataService.SysString(1153);
                    response = (index << 16) + 3;
                    break;
                case CardCommand.SSet:
                    text = DataService.SysString(1153);
                    response = (index << 16) + 4;
                    break;
                case CardCommand.Repos:
                    if ((Position & (int)CardPosition.FaceDown) > 0)
                    {
                        text = DataService.SysString(1154);
                    }
                    else if ((Position & (int)CardPosition.Attack) > 0)
                    {
                        text = DataService.SysString(1155);
                    }
                    else
                    {
                        text = DataService.SysString(1156);
                    }
                    response = (index << 16) + 2;
                    break;
                case CardCommand.Attack:
                    //attackIndicator = AttackIndicator.AddNew(RealPosition + new Vector3(0, 0.2f, 0), Controller);
                    text = DataService.SysString(1157);
                    response = (index << 16) + 1;
                    break;
                default:
                    text = "???";
                    break;
            }
            button.Set(text, response, Transform);
            buttons.Add(button);
            for (int i = 0; i < buttons.Count; ++i)
            {
                buttons[i].ButtonObject.transform.localPosition = new Vector3(0, -2 * (buttons.Count - 1) * 0.5f + i * 2, -0.1f);
            }
        }

        public void AddActivateCommand(int index, int description, int flag)
        {
            Debug.Log(string.Format("[{0}] wants to add activate, index={1}, flag={2}", DataService.GetName(Data.Code), index, flag));
            int response = 0;
            if (GameInfo.Instance.CurrentMessage == GameMessage.SelectIdleCmd)
            {
                response = (index << 16) + 5;
            }
            else if (GameInfo.Instance.CurrentMessage == GameMessage.SelectBattleCmd)
            {
                response = index << 16;
            }
            else
            {
                response = index;
            }
            if (flag == 1)
            {
                var btn = ButtonPool.New();
                btn.Set(DataService.SysString(1161), response, Transform);
                buttons.Add(btn);
            }
            else if (flag == 2)
            {
                var btn = ButtonPool.New();
                btn.Set(DataService.SysString(1162), response, Transform);
                buttons.Add(btn);
            }
            else
            {
                if (activateButton != null)
                {
                    activateButton.AddOption(new Tuple<int, int>(index, description));
                    return;
                }
                activateButton = ButtonPool.New();
                if (description != 0)
                {
                    activateButton.AddOption(new Tuple<int, int>(index, description));
                }
                activateButton.Set(DataService.SysString(1150), response, Transform);
                buttons.Add(activateButton);
            }
            for (int i = 0; i < buttons.Count; ++i)
            {
                buttons[i].ButtonObject.transform.localPosition = new Vector3(0, -2 * (buttons.Count - 1) * 0.5f + i * 2, -0.1f);
            }
        }

        public void ClearCommand()
        {
            ButtonPool.Store(buttons);
            buttons.Clear();
            activateButton = null;
            //if (attackIndicator != null)
            //{
            //    AttackIndicator.Collecet(attackIndicator);
            //    attackIndicator = null;
            //}
        }

        internal void UpdateInfo(BinaryReader reader)
        {
            int flag = reader.ReadInt32();
            if (flag == 0)
            {
                return;
            }
            int buffer;
            if ((flag & (int)Query.Code) > 0)
            {
                SetCode(reader.ReadUInt32());
            }
            if ((flag & (int)Query.Position) > 0)
            {
                buffer = (reader.ReadInt32() >> 24) & 0xff;
                if ((Location & 0x30) > 0 && Position != buffer)
                {
                    RealRotation = FieldInfo.FieldRotations[Controller][(CardPosition)buffer];
                    Transform.rotation = RealRotation;
                }
                Position = buffer;
            }
            if ((flag & (int)Query.Alias) > 0)
            {
                Data.Alias = reader.ReadUInt32();
            }
            if ((flag & (int)Query.Type) > 0)
            {
                Data.Type = reader.ReadUInt32();
            }
            if ((flag & (int)Query.Level) > 0)
            {
                buffer = reader.ReadInt32();
                if (Data.Level != buffer)
                {
                    Data.Level = (uint)buffer;
                    levelStr = buffer.ToString();
                }
            }
            if ((flag & (int)Query.Rank) > 0)
            {
                buffer = reader.ReadInt32();
                if (Data.Level != buffer && buffer > 0)
                {
                    Data.Level = (uint)buffer;
                    levelStr = buffer.ToString();
                }
            }
            if ((flag & (int)Query.Attribute) > 0)
            {
                Data.Attribute = reader.ReadUInt32();
            }
            if ((flag & (int)Query.Race) > 0)
            {
                Data.Race = reader.ReadUInt32();
            }
            if ((flag & (int)Query.Attack) > 0)
            {
                Data.Attack = reader.ReadInt32();
            }
            if ((flag & (int)Query.Defence) > 0)
            {
                Data.Defense = reader.ReadInt32();
            }
            if ((flag & (int)Query.BaseAttack) > 0)
            {
                Data.BaseAttack = reader.ReadInt32();
            }
            if ((flag & (int)Query.BaseDefence) > 0)
            {
                Data.BaseDefense = reader.ReadInt32();
            }
            if ((flag & (int)Query.Reason) > 0)
            {
                reader.ReadInt32();
            }
            if ((flag & (int)Query.ReasonCard) > 0)
            {
                reader.ReadInt32();
            }
            if ((flag & (int)Query.EquipCard) > 0)
            {
                var coord = reader.ReadCoordinate();
                reader.ReadByte();
                //todo equit target
            }
            if ((flag & (int)Query.TargetCard) > 0)
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    var coord = reader.ReadCoordinate();
                    reader.ReadByte();
                }
            }
            if ((flag & (int)Query.OverlayCard) > 0)
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    Overlay[i].SetCode(reader.ReadUInt32());
                }
            }
            if ((flag & (int)Query.Counters) > 0)
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    uint ctype = reader.ReadUInt16();
                    int cc = reader.ReadInt16();
                    Counters[ctype] = cc;
                }
            }
            if ((flag & (int)Query.Owner) > 0)
            {
                reader.ReadInt32();
            }
            if ((flag & 0x80000) > 0)
            {
                reader.ReadInt32();
            }
            if ((flag & (int)Query.LScale) > 0)
            {
                Data.LScale = reader.ReadUInt32();
            }
            if ((flag & (int)Query.RScale) > 0)
            {
                Data.RScale = reader.ReadUInt32();
            }
            if ((flag & (int)Query.Link) > 0)
            {
                buffer = reader.ReadInt32();
                if (Data.Level != buffer && buffer > 0)
                {
                    Data.Level = (uint)buffer;
                    levelStr = buffer.ToString();
                }
                Data.LinkMarker = reader.ReadUInt32();
            }
        }

        public void Update()
        {
            if (Location == 0x04 && (Position & 0x5) > 0)
            {
                if (infoPanel == null)
                {
                    infoPanel = PanelPool.New();
                }
                infoPanel.PanelObjecet.transform.position = RealPosition + new Vector3(0, 1, -4.4f);
                //todo fix color
                if (Data.Attack < Data.BaseAttack)
                {
                    infoPanel.TextAtk.text = "<color=#888888>" + (Data.Attack >= 0 ? Data.Attack.ToString() : "?") + "</color>";
                }
                else if (Data.Attack > Data.BaseAttack)
                {
                    infoPanel.TextAtk.text = "<color=#fffd45>" + (Data.Attack >= 0 ? Data.Attack.ToString() : "?") + "</color>";
                }
                else
                {
                    infoPanel.TextAtk.text = "<color=#ffffff>" + (Data.Attack >= 0 ? Data.Attack.ToString() : "?") + "</color>";
                }
                infoPanel.TextLevel.text = levelStr;
                if ((Data.Type & (int)CardType.Link) > 0)
                {
                    infoPanel.TextDef.text = "-";
                    //todo level link rank
                }
                else
                {
                    if (Data.Defense < Data.BaseDefense)
                    {
                        infoPanel.TextDef.text = "<color=#888888>" + (Data.Defense >= 0 ? Data.Defense.ToString() : "?") + "</color>";
                    }
                    else if (Data.Defense > Data.BaseDefense)
                    {
                        infoPanel.TextDef.text = "<color=#fffd45>" + (Data.Defense >= 0 ? Data.Defense.ToString() : "?") + "</color>";
                    }
                    else
                    {
                        infoPanel.TextDef.text = "<color=#ffffff>" + (Data.Defense >= 0 ? Data.Defense.ToString() : "?") + "</color>";
                    }
                }
            }
            else if (infoPanel != null)
            {
                PanelPool.Store(infoPanel);
                infoPanel = null;
            }
            if (!Selectable && !Selected)
            {
                decorator.localScale = Vector3.zero;
            }
            else
            {
                if (Selectable)
                {
                    decorator.localScale = Vector3.one;
                    decorator.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(.4f, .6887f, .8396f));
                }
                if (Selected)
                {
                    decorator.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(0.8392f, 0.4757f, 0.4f));
                    decorator.localScale = Vector3.one;
                }
            }
            if (buttons.Count > 0)
            {
                if ((Transform.parent != null || (Location & 0x71) == 0) && CheckMouse(false))
                {
                    buttons.ForEach((b) => b.Show());
                }
                else
                {
                    buttons.ForEach((b) => b.Hide());
                }
            }
        }

        internal void Detach()
        {
            if (Transform.parent != null)
            {
                GroupText.transform.localScale = Vector3.zero;
                Transform.SetParent(null);
            }
        }

        internal void SetCardHints(CardHint hint, uint value)
        {
            switch (hint)
            {
                case CardHint.DescriptionAdd:
                    if (DescriptionHints.ContainsKey(value))
                    {
                        DescriptionHints[value]++;
                    }
                    else
                    {
                        DescriptionHints.Add(value, 1);
                    }
                    break;
                case CardHint.DescriptionRemove:
                    DescriptionHints[value]--;
                    if (DescriptionHints[value] == 0)
                    {
                        DescriptionHints.Remove(value);
                    }
                    break;
                default:
                    //todo animation
                    if (CardHints.ContainsKey(hint))
                    {
                        CardHints[hint] = value;
                    }
                    else
                    {
                        CardHints.Add(hint, value);
                    }
                    break;
            }
        }

        #region behavior

        public void BecomeTarget()
        {
            HighLight();
            AddAnimation(Twinkle(), 0.4f);
        }

        internal void Untarget(ClientCard clientCardController)
        {
        }

        internal void Target(ClientCard target)
        {
        }

        internal void Unequip()
        {
        }

        internal void Equip(ClientCard equipment)
        {
        }

        internal void HighLight()
        {
        }

        #endregion

        #region enumerators
        internal IEnumerator ConfirmCloseup(float x, float duration)
        {
            Detach();
            floating = false;
            var tarPos = FieldInfo.ConfirmPosition[Controller];
            tarPos.x = x;
            yield return Transform.SmoothDampTo(tarPos, FieldInfo.HandRotations[CardPosition.FaceUp], 0.15f);
            yield return Animator.WaitTime(duration);
            yield return Transform.SmoothDampTo(RealPosition, RealRotation, 0.15f);
        }
        internal IEnumerator SingleConfirm(float duration, float delay = 0)
        {
            Detach();
            floating = false;
            if (delay > 0)
            {
                yield return Animator.WaitTime(delay);
            }
            yield return Transform.SmoothDampTo(
                 FieldInfo.ConfirmPosition[Controller], FieldInfo.HandRotations[CardPosition.FaceUp], 0.15f);
            yield return Animator.WaitTime(duration);
            yield return Transform.MoveAndRotateTo(RealPosition, RealRotation, 0.15f);
        }
        internal IEnumerator ShuffleHandMove(float duration)
        {
            Detach();
            yield return Transform.MoveAndRotateTo(
                new Vector3(0, 0, RealPosition.z), FieldInfo.HandRotations[CardPosition.FaceDown], duration / 2);
            yield return Transform.MoveAndRotateTo(RealPosition, RealRotation, duration / 2);
        }
        internal IEnumerator ShuffleSetMove()
        {
            yield return Transform.MoveAndRotateTo(
                FieldInfo.ShuffleSetPosition[Controller], FieldInfo.FieldRotations[Controller][CardPosition.FaceDown], 0.1f);
            yield return Animator.WaitTime(0.15f);
            yield return Transform.MoveAndRotateTo(RealPosition, RealRotation, 0.15f);
        }
        internal IEnumerator ChangeSequence()
        {
            Detach();
            var tarPos = RealPosition + new Vector3(Controller * 16 - 8, 0, 0);
            yield return Transform.SmoothDampTo(tarPos, RealRotation, 0.1f);
            yield return Transform.SmoothDampTo(RealPosition, RealRotation, 0.1f);
        }
        internal IEnumerator ChainingShowUp()
        {
            Detach();
            floating = false;
            if (Location == (int)CardLocation.Hand)
            {
                yield return Transform.MoveAndRotateTo(RealPosition, FieldInfo.HandRotations[CardPosition.FaceUp], 0.2f);
                yield return Animator.WaitTime(0.3f);
            }
            else if ((Location & 0x30) > 0)
            {
                var tarPos = RealPosition + new Vector3(Controller * 16 - 8, 0, 0);
                yield return Transform.SmoothDampTo(tarPos, RealRotation, 0.2f);
                yield return Animator.WaitTime(0.3f);
                yield return Transform.SmoothDampTo(RealPosition, RealRotation, 0.1f);
            }
            else
            {
                yield return Transform.MoveAndRotateTo(RealPosition, RealRotation, 0.1f);
                yield return Animator.WaitTime(0.4f);
            }
        }
        internal IEnumerator Swing()
        {
            var tarRot = FieldInfo.FieldRotations[Controller][CardPosition.FaceDownAttack];

            int flag = (Sequence & 1) == 0 ? 1 : -1;
            var tarPos1 = RealPosition + new Vector3(Random.value * 4 * flag, 0, 0);
            var tarPos2 = RealPosition + new Vector3(Random.value * 4 * flag * -1, 0, 0);
            for (int i = 0; i < 2; ++i)
            {
                yield return Transform.MoveAndRotateTo(tarPos1, tarRot, 0.1f);
                yield return Transform.MoveAndRotateTo(tarPos2, tarRot, 0.1f);
            }
            yield return Transform.MoveAndRotateTo(RealPosition, RealRotation, 0.1f);
        }
        internal IEnumerator MoveReal(float duration)
        {
            Detach();
            floating = false;
            yield return Transform.MoveAndRotateTo(RealPosition, RealRotation, duration);
        }
        internal IEnumerator MoveRealSmoothly(float duration)
        {
            floating = false;
            yield return Transform.SmoothDampTo(RealPosition, RealRotation, duration);
        }
        internal IEnumerator MoveQuery()
        {
            floating = false;
            yield return Transform.FreeSmoothDampTo(QueryingPosition, FieldInfo.HandRotations[CardPosition.FaceUp]);
        }
        private IEnumerator Twinkle()
        {
            //todo implement by material
            Detach();
            if (floating)
            {
                floating = false;
                yield return Transform.MoveAndRotateTo(RealPosition, RealRotation, 0.05f);
            }
            if ((Location & 0x30) > 0)
            {
                float xOffset = -10;
                if (Controller == 1)
                {
                    xOffset *= -1;
                }
                var tarPos = RealPosition + new Vector3(xOffset, 0, 0);
                yield return Transform.MoveTo(tarPos, 0.1f);
                for (int i = 0; i < 2; ++i)
                {
                    Transform.localScale = Vector3.zero;
                    yield return Animator.WaitTime(0.08f);
                    Transform.localScale = Vector3.one;
                    yield return Animator.WaitTime(0.08f);
                }
                yield return Transform.MoveAndRotateTo(RealPosition, RealRotation, 0.1f);
            }
            else
            {
                for (int i = 0; i < 2; ++i)
                {
                    Transform.localScale = Vector3.zero;
                    yield return Animator.WaitTime(0.08f);
                    Transform.localScale = Vector3.one;
                    yield return Animator.WaitTime(0.08f);
                }
            }
        }
        private IEnumerator FaceTowardsCamera()
        {
            floating = true;
            float elapsed = 0;
            Vector3 tarPos = Vector3.Lerp(RealPosition, MainGame.Instance.MainCamera.transform.position, 0.3f);
            var tarRot = FieldInfo.HandRotations[CardPosition.FaceUp];
            var iterator = Transform.SmoothDampTo(tarPos, tarRot, 0.15f);
            while (CheckMouse(elapsed < 0.6))
            {
                if (Transform.parent == null && (Location & 0xE) > 0 && elapsed < 0.2f)
                {
                    iterator.MoveNext();
                }
                elapsed += Time.fixedDeltaTime;
                yield return null;
            }
            floating = false;
            if (Transform.parent == null)
            {
                yield return Transform.SmoothDampTo(RealPosition, RealRotation, 0.1f);
            }
        }
        #endregion
        private bool CheckMouse(bool approximate)
        {
            var hit = MainGame.Instance.Field.HitTransform;
            if (hit != null && (hit == faceObject.transform || hit.CompareTag("CommandButton") && hit.parent.parent == Transform))
            {
                return true;
            }
            if (approximate)
            {
                var cardpos = MainGame.Instance.MainCamera.WorldToScreenPoint(Transform.position);
                var mpos = Input.mousePosition;
                return Mathf.Abs(cardpos.x - mpos.x) < 50 && Mathf.Abs(cardpos.y - mpos.y) < 70;
            }
            return false;
        }

        private void OnClick(BaseEventData baseEventData)
        {
            if (Transform.parent == null &&
                ((Location & 0x30) > 0 || ((Location & 0x40) > 0 && Controller == 0 && GameInfo.Instance.PlayerType != PlayerType.Observer)))
            {
                MainGame.Instance.Field.Query(MainGame.Instance.Field.GetCards(Controller, (CardLocation)Location));
            }
            else if (Selectable)
            {
                MainGame.Instance.Field.OnCardSelected(this);
            }
            else if (GameInfo.Instance.CurrentMessage == GameMessage.SelectChain)
            {
                activateButton?.Callback(null);
            }
            else if (GameInfo.Instance.CurrentMessage == GameMessage.SelectBattleCmd && activateButton != null && buttons.Count == 1)
            {
                buttons[0].Callback(null);
            }
            else if (Overlay.Count != 0 && !Selectable)
            {
                if (Overlay[0].Transform.parent != null)
                {
                    MainGame.Instance.Field.EndQuery();
                }
                else
                {
                    MainGame.Instance.Field.Query(Overlay);
                }
            }
        }

        private void OnPointerEnter(BaseEventData baseEventData)
        {
            if (Data.Code > 0)
            {
                MainGame.Instance.Descriptor.Refresh(Data.Code);
                if (CardHints.Count > 0 || DescriptionHints.Count > 0 || Counters.Count > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    if (CardHints.Count > 0 && (Location & (int)CardLocation.OnField) > 0)
                    {
                        foreach (var hint in CardHints.Keys)
                        {
                            uint val = CardHints[hint];
                            if (val <= 0)
                            {
                                continue;
                            }
                            builder.Append(DataService.SysString(210 + (uint)hint));
                            switch (hint)
                            {
                                case CardHint.Turn:
                                case CardHint.Number:
                                    builder.Append(val);
                                    break;
                                case CardHint.Card:
                                    builder.Append(DataService.GetName(val));
                                    break;
                                case CardHint.Race:
                                    builder.Append(DataService.FormatRace(val));
                                    break;
                                case CardHint.Attribute:
                                    builder.Append(DataService.FormatAttribute(val));
                                    break;
                                default:
                                    break;
                            }
                            builder.Append("\n");
                        }
                    }
                    if (DescriptionHints.Count > 0)
                    {
                        foreach (var desc in DescriptionHints.Keys)
                        {
                            builder.Append(DataService.GetDescription(desc)).Append("\n");
                        }
                    }
                    if (Counters.Count > 0)
                    {
                        foreach (var key in Counters.Keys)
                        {
                            builder.Append("[").Append(DataService.CounterName(key)).Append("]:").Append(Counters[key]).Append("\n");
                        }
                    }
                    MainGame.Instance.Descriptor.ShowPanel(builder.ToString());
                }
            }
            if (!floating && animation.IsCompleted && (Location & 0x71) == 0)
            {
                AddAnimation(FaceTowardsCamera(), 0);
            }
        }

        private void OnPointerExit(BaseEventData obj)
        {
            MainGame.Instance.Descriptor.HidePanel();
        }

        internal void AddAnimation(IEnumerator anim, float waitTime = 10000)
        {
            if (animation.IsCompleted)
            {
                animation.Insert(anim, waitTime);
                Animator.Instance.Play(animation);
            }
            else
            {
                animation.Insert(anim, waitTime);
            }
        }
    }
}