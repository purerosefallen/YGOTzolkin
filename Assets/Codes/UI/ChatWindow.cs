using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YGOTzolkin.Service;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.UI
{
    class ChatWindow : WindowBase
    {
        private readonly TMP_InputField iptMessage;
        private readonly Button btnSend;
        private readonly RectTransform msgBoard;

        private readonly Queue<TextMeshProUGUI> msgQueue;
        private readonly Stack<TextMeshProUGUI> msgPool;
        private readonly Dictionary<TextMeshProUGUI, float> messageTimers;
        private readonly List<TextMeshProUGUI> allMessage;
        private readonly int msgCount;
        private readonly float duration;
        private float msgHeight;


        public ChatWindow()
        {
            MainCanvas = Utility.Tools.LoadResource<Canvas>("Prefabs/ChatWindow");
            msgBoard = GetControl<RectTransform>("MesageBoard");
            iptMessage = GetControl<TMP_InputField>("IptMessage");
            btnSend = GetControl<Button>("BtnSend");
            btnSend.onClick.AddListener(OnSend);
            iptMessage.onSubmit.AddListener((p) => OnSend());

            msgCount = 4;
            duration = 10;
            msgQueue = new Queue<TextMeshProUGUI>(msgCount);
            msgPool = new Stack<TextMeshProUGUI>(msgCount);
            messageTimers = new Dictionary<TextMeshProUGUI, float>(msgCount);
            allMessage = new List<TextMeshProUGUI>();
            for (int i = 0; i < msgCount; ++i)
            {
                var msg = Utility.Tools.LoadResource<TextMeshProUGUI>("Prefabs/ChatMessage");
                msg.text = string.Empty;
                msgPool.Push(msg);
                messageTimers.Add(msg, 0);
                allMessage.Add(msg);
            }
            msgHeight = allMessage[0].rectTransform.rect.height;
        }

        internal override void Show()
        {
            MainGame.Instance.FrameActions.Add(Update);
            base.Show();
        }

        internal override void Hide()
        {
            MainGame.Instance.FrameActions.Remove(Update);
            base.Hide();
            while (msgQueue.Count > 0)
            {
                var msg = msgQueue.Dequeue();
                msg.rectTransform.SetParent(null, false);
                msgPool.Push(msg);
            }
        }

        internal void AddMessage(int sender, string content)
        {
            //todo color setting
            StringBuilder builder = new StringBuilder();
            switch (sender)
            {
                case 0: //from host
                    builder.Append(GameInfo.Instance.HostName).Append(": ");
                    break;
                case 1: //from client
                    builder.Append(GameInfo.Instance.ClientName).Append(": ");
                    break;
                case 2: //host tag
                    builder.Append(GameInfo.Instance.HostNameTag).Append(": ");
                    break;
                case 3: //client tag
                    builder.Append(GameInfo.Instance.ClientNameTag).Append(": ");
                    break;
                case 7: //local name
                    builder.Append(GameInfo.Instance.LocalName).Append(": ");
                    break;
                case 8: //system custom message, no prefix.
                    builder.Append("[System]: ");
                    break;
                case 9: //error message
                    builder.Append("[Script Error]: ");
                    break;
                default: //from watcher or unknown
                    if (sender < 11 || sender > 19)
                    {
                        builder.Append("[---]: ");
                    }
                    break;
            }
            builder.Append(content);

            TextMeshProUGUI msg;
            if (msgQueue.Count == msgCount)
            {
                msg = msgQueue.Dequeue();
            }
            else
            {
                msg = msgPool.Pop();
                msg.rectTransform.SetParent(msgBoard, false);
            }
            messageTimers[msg] = 0;
            msg.text = builder.ToString();
            msgQueue.Enqueue(msg);
            Reposition();
        }

        private void Update()
        {
            foreach (var msg in allMessage)
            {
                if (msg.rectTransform.parent != null)
                {
                    messageTimers[msg] += Time.fixedDeltaTime;
                    if (messageTimers[msg] > duration)
                    {
                        msg.rectTransform.SetParent(null, false);
                        msgPool.Push(msg);
                        msgQueue.Dequeue();
                        Reposition();
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                iptMessage.ActivateInputField();
            }
        }

        private void Reposition()
        {
            int c = msgQueue.Count;
            for (int i = 0; i < c; ++i)
            {
                msgQueue.ElementAt(c - 1 - i).rectTransform.anchoredPosition3D = new Vector3(0, msgHeight * i, 0);
            }
        }

        private void OnSend()
        {
            if (string.IsNullOrEmpty(iptMessage.text))
            {
                return;
            }
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            byte[] unicode = Encoding.Unicode.GetBytes(iptMessage.text + '\0');
            writer.Write((short)(unicode.Length + 1));
            writer.Write((byte)CToSMessage.Chat);
            writer.Write(unicode);
            NetworkService.Instance.Send(stream.ToArray());
            iptMessage.text = string.Empty;
        }

    }
}
