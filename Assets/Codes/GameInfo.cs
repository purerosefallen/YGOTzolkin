
using YGOTzolkin.YGOModel;

namespace YGOTzolkin
{
    class GameInfo
    {
        private static readonly object lockObject = new object();
        private static GameInfo instance;
        public static GameInfo Instance
        {
            private set { instance = value; }
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new GameInfo();
                        }
                        return instance;
                    }
                }
                return instance;
            }
        }

        private GameInfo()
        {
            SelfType = 0;
            WatchingCount = 0;
            MatchKill = 0;
        }

        public string HostName { get; set; }
        public string ClientName { get; set; }
        public string HostNameTag { get; set; }
        public string ClientNameTag { get; set; }
        public string LocalName { get;set; }

        public byte SelfType { get; set; }
        public PlayerType PlayerType { get; set; }
        public bool IsHost { get; set; }
        public bool IsFirst { get; set; }
        public bool IsTag { get; set; }
        public int DuelRule { get; set; }
        public ushort TimeLimit { get; set; }
        public int WatchingCount { get; set; }

        public int MatchKill { get; set; }
        public ushort[] TimeLeft = new ushort[2];
        public int[] Lp = new int[2];
        public bool[] TagPlayer = new bool[2];

        public GameMessage CurrentMessage { get; set; }
        public int Turn { get; set; }
        public bool IsDeckReversed { get; set; }
        public string EventTiming { get; set; }
        public uint SelectHint { get; set; }

        public bool IsReplaying { get; set; }
        public bool IsReplaySkiping { get; set; }
        public bool IsReplaySwapped { get; set; }


        public int LocalPlayer(int player)
        {
            return IsFirst ? player : 1 - player;
        }
    }
}
