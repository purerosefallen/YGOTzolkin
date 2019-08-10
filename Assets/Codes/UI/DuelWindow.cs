using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YGOTzolkin.Service;
using YGOTzolkin.Utility;
using YGOTzolkin.YGOModel;
using static YGOTzolkin.Service.DataService;

namespace YGOTzolkin.UI
{
    class DuelWindow : LazyWindow
    {
        public Toggle TglChainIngnore { get; private set; }
        public Toggle TglChainWhenAvail { get; private set; }
        public Toggle TglChainAlways { get; private set; }

        private Button btnSurrender;
        private Button btnBP;
        private Button btnM2;
        private Button btnEP;
        private Button btnShuffle;
        private Button btnCancelFinish;
        private Button btnActions;

        private RawImage imgCloseup;
        private TextMeshProUGUI tMPPhase;
        private TextMeshProUGUI tMPLp;
        private TextMeshProUGUI tMPHint;
        private TextMeshProUGUI tMPLP0;
        private TextMeshProUGUI tMPLP1;
        private TextMeshProUGUI tMPName0;
        private TextMeshProUGUI tMPName1;

        private List<TextMeshProUGUI> lpTexts;

        public DuelWindow()
        {

        }

        internal override void Show()
        {
            base.Show();
            SetVisible(btnShuffle.transform, false);
            SetVisible(btnBP.transform, false);
            SetVisible(btnEP.transform, false);
            SetVisible(btnM2.transform, false);
            SetVisible(btnCancelFinish.transform, false);
            SetVisible(btnActions.transform, false);
            SetVisible(tMPHint.transform, false);
            MainGame.Instance.FrameActions.Add(Update);
            if (GameInfo.Instance.PlayerType == PlayerType.Observer)
            {
                SetVisible(btnSurrender.transform, true);
                btnSurrender.SetButtonName(SysString(1350));
                SetVisible(TglChainAlways.transform, false);
                SetVisible(TglChainIngnore.transform, false);
                SetVisible(TglChainWhenAvail.transform, false);
            }
            else
            {
                SetVisible(TglChainAlways.transform, true);
                SetVisible(TglChainIngnore.transform, true);
                SetVisible(TglChainWhenAvail.transform, true);
                SetVisible(btnSurrender.transform, false);
            }
            imgCloseup.transform.localScale = Vector3.zero;
            tMPLp.alpha = 0;
            tMPPhase.alpha = 0;
            tMPName0.text = GameInfo.Instance.HostName;
            tMPName1.text = GameInfo.Instance.ClientName;
            lpTexts[0].text = "0";
            lpTexts[1].text = "0";
        }

        internal override void Hide()
        {
            base.Hide();
            MainGame.Instance.FrameActions.Remove(Update);
        }

        protected override void LazyInitialize()
        {
            MainCanvas = Tools.LoadResource<Canvas>("Prefabs/DuelWindow");

            TglChainIngnore = GetControl<Toggle>("TglChainIngnore");
            TglChainWhenAvail = GetControl<Toggle>("TglChainWhenAvail");
            TglChainAlways = GetControl<Toggle>("TglChainAlways");
            TglChainIngnore.SetToggleName(SysString(1292));
            TglChainWhenAvail.SetToggleName(SysString(1294));
            TglChainAlways.SetToggleName(SysString(1293));

            TryGetControl(out btnBP, "BtnBP");
            TryGetControl(out btnM2, "BtnM2");
            TryGetControl(out btnEP, "BtnEP");
            TryGetControl(out btnShuffle, "BtnShuffle");
            TryGetControl(out btnCancelFinish, "BtnCancelFinish");
            TryGetControl(out btnSurrender, "BtnSurrender");
            TryGetControl(out btnActions, nameof(btnActions));
            btnActions.onClick.AddListener(OnActions);
            btnActions.SetButtonName(TzlString(16));
            btnShuffle.SetButtonName(SysString(1297));
            btnShuffle.onClick.AddListener(OnShuffle);
            btnCancelFinish.onClick.AddListener(CancelOrFinish);
            btnBP.onClick.AddListener(OnBPClick);
            btnM2.onClick.AddListener(OnM2Click);
            btnEP.onClick.AddListener(OnEPClick);
            btnSurrender.onClick.AddListener(OnSurrenderClick);

            TryGetControl(out imgCloseup, "ImgCloseup");
            TryGetControl(out tMPPhase, "TMPPhase");
            TryGetControl(out tMPLp, "TMPLp");
            TryGetControl(out tMPLP0, "TMPLP0");
            TryGetControl(out tMPLP1, "TMPLP1");
            TryGetControl(out tMPName0, "TMPName0");
            TryGetControl(out tMPName1, "TMPName1");
            TryGetControl(out tMPHint, "TMPHint");
            lpTexts = new List<TextMeshProUGUI>
            {
                tMPLP0,
                tMPLP1,
            };
        }

        internal void FirstAwake()
        {
            if (!GameInfo.Instance.IsTag && !GameInfo.Instance.IsReplaying && GameInfo.Instance.PlayerType != PlayerType.Observer)
            {
                btnSurrender.SetButtonName(SysString(1351));
                SetVisible(btnSurrender.transform, true);
            }
            lpTexts[0].text = GameInfo.Instance.Lp[0].ToString();
            lpTexts[1].text = GameInfo.Instance.Lp[1].ToString();
        }

        internal void ShowSelectHint(string hint)
        {
            tMPHint.text = hint;
            SetVisible(tMPHint.transform, true);
        }

        internal void OnResponsed()
        {
            SetVisible(tMPHint.transform, false);
            SetVisible(btnActions.transform, false);
            if (GameInfo.Instance.CurrentMessage == GameMessage.SelectBattleCmd)
            {
                SetVisible(btnM2.transform, false);
                SetVisible(btnEP.transform, false);
            }
            else if (GameInfo.Instance.CurrentMessage == GameMessage.SelectIdleCmd)
            {
                SetVisible(btnBP.transform, false);
                SetVisible(btnEP.transform, false);
            }
        }

        internal void WhenSelectBattleCmd(bool canM2, bool canEp)
        {
            SetVisible(btnM2.transform, canM2);
            SetVisible(btnEP.transform, canEp);
        }

        internal void WhenSelectIdleCmd(bool canBp, bool canEp, bool canShuffle)
        {
            SetVisible(btnBP.transform, canBp);
            SetVisible(btnEP.transform, canEp);
            SetVisible(btnShuffle.transform, canShuffle);
            SetVisible(btnActions.transform, MainGame.Instance.Field.InteractiveCards.Count > 0);
        }

        internal void SetCancelOrFinish(int i)
        {
            if (i == 0)
            {
                SetVisible(btnCancelFinish.transform, false);
            }
            else if (i == 1)
            {
                SetVisible(btnCancelFinish.transform, true);
                btnCancelFinish.SetButtonName(SysString(1295));
            }
            else
            {
                SetVisible(btnCancelFinish.transform, true);
                btnCancelFinish.SetButtonName(SysString(1296));
            }
        }

        internal void NewTurn(int player)
        {
            GameInfo.Instance.Turn++;
            tMPPhase.text = "Next Turn";
            Animator.Instance.Play(new DuelAnimation(PhaseTextMove()));
            if (GameInfo.Instance.IsTag && GameInfo.Instance.Turn != 1)
            {
                if (player == 0)
                {
                    GameInfo.Instance.TagPlayer[0] = !GameInfo.Instance.TagPlayer[0];
                }
                else
                {
                    GameInfo.Instance.TagPlayer[1] = !GameInfo.Instance.TagPlayer[1];
                }
                if (!GameInfo.Instance.TagPlayer[0])
                {
                    tMPName0.text = GameInfo.Instance.HostName;
                }
                else
                {
                    tMPName0.text = GameInfo.Instance.HostNameTag;
                }
                if (!GameInfo.Instance.TagPlayer[1])
                {
                    tMPName1.text = GameInfo.Instance.ClientName;
                }
                else
                {
                    tMPName1.text = GameInfo.Instance.ClientNameTag;
                }
            }
        }

        internal void NewPhase(int phase)
        {
            if (phase == (int)DuelPhase.BattleStart)
            {
                tMPPhase.text = "Battle";
            }
            else
            {
                tMPPhase.text = ((DuelPhase)phase).ToString();
            }
            SetVisible(btnBP.transform, false);
            SetVisible(btnM2.transform, false);
            SetVisible(btnEP.transform, false);
            SetVisible(btnShuffle.transform, false);
            Animator.Instance.Play(new DuelAnimation(PhaseTextMove()));
        }

        internal void ChainingCloseup(uint code, float duration = 0.5f)
        {
            Animator.Instance.Play(new DuelAnimation(ShowCloseup(code, duration)));
        }

        internal void DisabledCloseup(uint code)
        {
            Animator.Instance.Play(new DuelAnimation(ShowCloseup(code, 0.5f, true)));
        }

        internal void LpChange(int val, int player, bool isRecovery = false)
        {
            lpTexts[player].text = GameInfo.Instance.Lp[player].ToString();
            Animator.Instance.Play(new DuelAnimation(ShowLpChange(val, player, isRecovery)));
        }

        internal void LpUpdate(int player)
        {
            lpTexts[player].text = GameInfo.Instance.Lp[player].ToString();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                TglChainAlways.isOn = true;
            }
            if (Input.GetKeyUp(KeyCode.A))
            {
                TglChainAlways.isOn = false;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                TglChainIngnore.isOn = true;
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                TglChainIngnore.isOn = false;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                TglChainWhenAvail.isOn = true;
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                TglChainWhenAvail.isOn = false;
            }
            if (Input.GetMouseButtonDown(1))
            {
                CancelOrFinish();
            }
        }

        private void CancelOrFinish()
        {
            switch (GameInfo.Instance.CurrentMessage)
            {
                case GameMessage.SelectChain:
                    if (!MainGame.Instance.Duel.IsChainForced)
                    {
                        MainGame.Instance.SendCToSResponse(-1);
                    }
                    break;
                case GameMessage.SortCard:
                case GameMessage.SortChain:
                    MainGame.Instance.SendCToSResponse(-1);
                    MainGame.Instance.Field.ResetSelection();
                    break;
                case GameMessage.SelectYesNo:
                case GameMessage.SelectEffectYN:
                    MainGame.Instance.ConfirmWindow.Cancel();
                    break;
                case GameMessage.SelectCard:
                case GameMessage.SelectUnSelectCard:
                case GameMessage.SelectSum:
                case GameMessage.SelectTribute:
                    if (GameInfo.Instance.CurrentMessage == GameMessage.SelectUnSelectCard)
                    {
                        if (MainGame.Instance.Field.Finishable || MainGame.Instance.Field.Cancelabel)
                        {
                            MainGame.Instance.SendCToSResponse(-1);
                            MainGame.Instance.Field.ResetSelection();
                        }
                    }
                    else
                    {
                        if (MainGame.Instance.Field.Finishable)
                        {
                            MainGame.Instance.Field.SendSelectResponse();
                        }
                        else if (MainGame.Instance.Field.Cancelabel)
                        {
                            MainGame.Instance.SendCToSResponse(-1);
                            MainGame.Instance.Field.ResetSelection();
                        }
                    }
                    break;
                case GameMessage.SelectIdleCmd:
                    MainGame.Instance.CurrentWindow.Hide();
                    break;
                default: break;
            }
            SetCancelOrFinish(0);
        }

        #region events
        private void OnBPClick()
        {
            MainGame.Instance.SendCToSResponse(6);
        }

        private void OnEPClick()
        {
            if (GameInfo.Instance.CurrentMessage == GameMessage.SelectBattleCmd)
            {
                MainGame.Instance.SendCToSResponse(3);
            }
            else
            {
                MainGame.Instance.SendCToSResponse(7);
            }
        }

        private void OnM2Click()
        {
            MainGame.Instance.SendCToSResponse(2);
        }

        private void OnSurrenderClick()
        {
            NetworkService.Instance.Send(CToSMessage.Surrender);
        }

        private void OnShuffle()
        {
            MainGame.Instance.SendCToSResponse(8);
        }

        private void OnActions()
        {
            MainGame.Instance.Field.SpreadAvailable();
        }
        #endregion

        private IEnumerator ShowCloseup(uint code, float duration, bool disabled = false)
        {
            imgCloseup.texture = TextureService.GetCardTexture(code);
            imgCloseup.transform.localScale = Vector3.one;
            if (disabled)
            {

            }
            else
            {
                yield return Animator.WaitTime(duration);
            }
            imgCloseup.transform.localScale = Vector3.zero;
            imgCloseup.texture = null;
        }

        private IEnumerator ShowLpChange(int val, int player, bool isRecovery)
        {
            if (isRecovery)
            {
                tMPLp.text = "+" + val.ToString();
            }
            else
            {
                tMPLp.text = "-" + val.ToString();
            }
            if (player == 0)
            {
                tMPLp.rectTransform.localPosition = new Vector3(0, -100, 0);
            }
            else
            {
                tMPLp.rectTransform.localPosition = new Vector3(0, 100, 0);
            }
            tMPLp.alpha = 0;
            float elapsed = 0;
            while (elapsed <= 0.1f)
            {
                elapsed += Time.deltaTime;
                tMPLp.alpha = Mathf.Lerp(0, 1, elapsed / 0.1f);
                yield return null;
            }
            yield return Animator.WaitTime(0.5f);
            elapsed = 0;
            while (elapsed <= 0.1f)
            {
                elapsed += Time.deltaTime;
                tMPLp.alpha = Mathf.Lerp(1, 0, elapsed / 0.1f);
                yield return null;
            }
        }

        private IEnumerator PhaseTextMove()
        {
            var orgPos = tMPPhase.rectTransform.localPosition;
            var endPos = new Vector3(orgPos.x * -1, orgPos.y, orgPos.z);
            var elapsed = 0.0f;
            while (elapsed <= 0.1f)
            {
                elapsed += Time.deltaTime;
                tMPPhase.rectTransform.localPosition = Vector3.Lerp(orgPos, Vector3.zero, elapsed / 0.1f);
                tMPPhase.alpha = Mathf.Lerp(0, 1, elapsed / 0.1f);
                yield return null;
            }
            yield return Animator.WaitTime(0.5f);
            elapsed = 0.0f;
            while (elapsed <= 0.1f)
            {
                elapsed += Time.deltaTime;
                tMPPhase.rectTransform.localPosition = Vector3.Lerp(Vector3.zero, endPos, elapsed / 0.1f);
                tMPPhase.alpha = Mathf.Lerp(1, 0, elapsed / 0.1f);
                yield return null;
            }
            tMPPhase.rectTransform.localPosition = orgPos;
        }

    }
}
