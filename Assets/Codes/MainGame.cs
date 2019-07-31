using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using YGOTzolkin.Service;
using YGOTzolkin.UI;
using YGOTzolkin.Utility;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin
{
    class MainGame : MonoBehaviour
    {
        //main component
        public Camera MainCamera;
        public EventSystem EventSystem;

        private readonly Vector3 duelCameraPosition = new Vector3(0, 105, -63);
        private readonly Vector3 buildCameraPosition = new Vector3(0, 98, -4);

        //delay loading
        public WindowBase CurrentWindow { get; private set; }
        private Lazy<OptionSelector> opSelector;
        public OptionSelector OptionSelector
        {
            get
            {
                CurrentWindow = opSelector.Value;
                return opSelector.Value;
            }
        }
        private Lazy<ImageSelector> imgSelector;
        public ImageSelector ImageSelector
        {
            get
            {
                CurrentWindow = imgSelector.Value;
                return imgSelector.Value;
            }
        }
        private Lazy<ConfirmWindow> confirmWindow;
        public ConfirmWindow ConfirmWindow
        {
            get
            {
                CurrentWindow = confirmWindow.Value;
                return confirmWindow.Value;
            }
        }
        private Lazy<AnnounceWindow> annWindow;
        public AnnounceWindow AnnounceWindow
        {
            get
            {
                CurrentWindow = annWindow.Value;
                return annWindow.Value;
            }
        }
        private Lazy<InputBox> inputBox;
        public InputBox InputBox
        {
            get
            {
                CurrentWindow = inputBox.Value;
                return inputBox.Value;
            }
        }
        private Lazy<ToggleSelector> tglSelector;
        public ToggleSelector ToggleSelector
        {
            get
            {
                CurrentWindow = tglSelector.Value;
                return tglSelector.Value;
            }
        }
        private Lazy<HintBox> hint;
        public HintBox HintBox
        {
            get
            {
                CurrentWindow = hint.Value;
                return hint.Value;
            }
        }
        private Lazy<CardDescriptor> lazyDescriptor;
        public CardDescriptor Descriptor
        {
            get { return lazyDescriptor.Value; }
        }
        private Lazy<ChatWindow> lazyChat;
        public ChatWindow ChatWindow
        {
            get { return lazyChat.Value; }
        }
        //runtime init
        public ToolStrip ToolStrip { get; private set; }
        public DuelWindow DuelWindow { get; private set; }
        public MainMenu Menu { get; private set; }
        public ServerWindow ServerWindow { get; private set; }
        public ConfigWindow ConfigWindow { get; private set; }
        public RoomWindow RoomWindow { get; private set; }
        public DeckBuilder DeckBuilder { get; private set; }

        public Field Field { get; private set; }
        public ClientDuel Duel { get; private set; }
        public Queue<Action> AsyncTasks { get; private set; }
        public MapList<Action> FrameActions { get; private set; }


        public static MainGame Instance { get; private set; }

        public void Awake()
        {
            Application.targetFrameRate = 60;
        }

        public void Start()
        {
            Instance = this;
            AsyncTasks = new Queue<Action>(300);
            FrameActions = new MapList<Action>(300);

            Config.InitConfig();
            DataService.LoadData();
            TextureService.LoadTextures();
            DeckService.LoadForbiddenLists();

            imgSelector = new Lazy<ImageSelector>();
            tglSelector = new Lazy<ToggleSelector>();
            confirmWindow = new Lazy<ConfirmWindow>();
            annWindow = new Lazy<AnnounceWindow>();
            opSelector = new Lazy<OptionSelector>();
            inputBox = new Lazy<InputBox>();
            hint = new Lazy<HintBox>();
            lazyDescriptor = new Lazy<CardDescriptor>();
            lazyChat = new Lazy<ChatWindow>();

            ToolStrip = new ToolStrip();
            DuelWindow = new DuelWindow();
            DeckBuilder = new DeckBuilder();
            RoomWindow = new RoomWindow();
            Menu = new MainMenu();
            ConfigWindow = new ConfigWindow();
            ServerWindow = new ServerWindow();

            Field = new Field();
            Duel = new ClientDuel();

            Menu.Show();
        }

        public void Update()
        {
            if (AsyncTasks.Count > 0)
            {
                int i = 0;
                while (i++ < 2 && AsyncTasks.Count > 0)
                {
                    AsyncTasks.Dequeue().Invoke();
                }
            }
            Animator.Instance.Update();
            Duel.Update();

            foreach (var act in FrameActions)
            {
                act();
            }
        }

        public void OnApplicationQuit()
        {
            Config.Save();
        }

        internal void SendCToSResponse(int v)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((short)5);
            writer.Write((byte)CToSMessage.Response);
            writer.Write(v);
            NetworkService.Instance.Send(stream.ToArray());
            Field.ClearCommands();
            DuelWindow.OnResponsed();
            Debug.Log("-->ctos:Response:" + v.ToString());
        }

        internal void SendCToSResponse(byte[] content)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((short)(1 + content.Length));
            writer.Write((byte)CToSMessage.Response);
            writer.Write(content);
            NetworkService.Instance.Send(stream.ToArray());
            Field.ClearCommands();
            DuelWindow.OnResponsed();
            Debug.Log("-->ctos:Response:" + ByteExtension.ByteToString(content));
        }

        internal void CameraToDuel()
        {
            if (MainCamera.transform.position != duelCameraPosition)
            {
                Animator.Instance.Play(new DuelAnimation(MainCamera.transform.MoveAndRotateTo(duelCameraPosition,
                    Quaternion.Euler(60, 0, 0), 0.2f), 0));
            }
        }

        internal void CameraToBuilder()
        {
            if (MainCamera.transform.position != buildCameraPosition)
            {
                Animator.Instance.Play(new DuelAnimation(MainCamera.transform.MoveAndRotateTo(buildCameraPosition,
                    Quaternion.Euler(90.0001f, 0, 0), 0.2f), 0));
            }
        }
    }
}
