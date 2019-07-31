﻿namespace YGOTzolkin.YGOModel
{
    public enum CardCommand
    {
        Activate = 0x0001,
        Summon = 0x0002,
        SpSummon = 0x0004,
        MSet = 0x0008,
        SSet = 0x0010,
        Repos = 0x0020,
        Attack = 0x0040,
        List = 0x0080,
        Operation = 0x0100,
        Reset = 0x0200,
    }
    public enum CardLocation
    {
        Deck = 0x01,
        Hand = 0x02,
        MonsterZone = 0x04,
        SpellZone = 0x08,
        Grave = 0x10,
        Removed = 0x20,
        Extra = 0x40,
        Overlay = 0x80,
        OnField = 0x0C,
        FZone = 0x100,
        PZone = 0x200,
    }
    public enum CardPosition
    {
        FaceUpAttack = 0x1,
        FaceDownAttack = 0x2,
        FaceUpDefence = 0x4,
        FaceDownDefence = 0x8,
        FaceUp = 0x5,
        FaceDown = 0xA,
        Attack = 0x3,
        Defence = 0xC,
    }
    public enum CToSMessage
    {
        Response = 0x1,
        UpdateDeck = 0x2,
        HandResult = 0x3,
        TpResult = 0x4,
        PlayerInfo = 0x10,
        CreateGame = 0x11,
        JoinGame = 0x12,
        LeaveGame = 0x13,
        Surrender = 0x14,
        TimeConfirm = 0x15,
        Chat = 0x16,
        HsToDuelist = 0x20,
        HsToObserver = 0x21,
        HsReady = 0x22,
        HsNotReady = 0x23,
        HsKick = 0x24,
        HsStart = 0x25
    }
    public enum GameMessage
    {
        Retry = 1,
        Hint = 2,
        Waiting = 3,
        Start = 4,
        Win = 5,
        UpdateData = 6,
        UpdateCard = 7,
        RequestDeck = 8,
        SelectBattleCmd = 10,
        SelectIdleCmd = 11,
        SelectEffectYN = 12,
        SelectYesNo = 13,
        SelectOption = 14,
        SelectCard = 15,
        SelectChain = 16,
        SelectPlace = 18,
        SelectPosition = 19,
        SelectTribute = 20,
        SortChain = 21,
        SelectCounter = 22,
        SelectSum = 23,
        SelectDisfield = 24,
        SortCard = 25,
        SelectUnSelectCard = 26,
        ConfirmDecktop = 30,
        ConfirmCards = 31,
        ShuffleDeck = 32,
        ShuffleHand = 33,
        RefreshDeck = 34,
        SwapGraveDeck = 35,
        ShuffleSetCard = 36,
        ReverseDeck = 37,
        DeckTop = 38,
        ShuffleExtra = 39,
        NewTurn = 40,
        NewPhase = 41,
        ConfirmExtraTop = 42,
        Move = 50,
        PosChange = 53,
        Set = 54,
        Swap = 55,
        FieldDisabled = 56,
        Summoning = 60,
        Summoned = 61,
        SpSummoning = 62,
        SpSummoned = 63,
        FlipSummoning = 64,
        FlipSummoned = 65,
        Chaining = 70,
        Chained = 71,
        ChainSolving = 72,
        ChainSolved = 73,
        ChainEnd = 74,
        ChainNegated = 75,
        ChainDisabled = 76,
        CardSelected = 80,
        RandomSelected = 81,
        BecomeTarget = 83,
        Draw = 90,
        Damage = 91,
        Recover = 92,
        Equip = 93,
        LpUpdate = 94,
        Unequip = 95,
        CardTarget = 96,
        CancelTarget = 97,
        PayLpCost = 100,
        AddCounter = 101,
        RemoveCounter = 102,
        Attack = 110,
        Battle = 111,
        AttackDiabled = 112,
        DamageStepStart = 113,
        DamageStepEnd = 114,
        MissedEffect = 120,
        BeChainTarget = 121,
        CreateRelation = 122,
        ReleaseRelation = 123,
        TossCoin = 130,
        TossDice = 131,
        RockPaperScissors = 132,
        HandResult = 133,
        AnnounceRace = 140,
        AnnounceAttribute = 141,
        AnnounceCard = 142,
        AnnounceNumber = 143,
        AnnounceCardFilter = 144,
        CardHint = 160,
        TagSwap = 161,
        ReloadField = 162,
        AiName = 163,
        ShowHint = 164,
        PlayerHint = 165,
        MatchKill = 170,
        CustomMsg = 180,
    }
    public enum LinkMarker
    {
        BottomLeft = 1,
        Bottom = 2,
        BottomRight = 4,
        Left = 8,
        Right = 32,
        TopLeft = 64,
        Top = 128,
        TopRight = 256
    }
    public enum SToCMessage
    {
        GameMessage = 0x1,
        ErrorMessage = 0x2,
        SelectHand = 0x3,
        SelectTp = 0x4,
        HandResult = 0x5,
        TpResult = 0x6,
        ChangeSide = 0x7,
        WaitingSide = 0x8,
        //CreateGame = 0x11,
        JoinGame = 0x12,
        TypeChange = 0x13,
        //LeaveGame = 0x14,
        DuelStart = 0x15,
        DuelEnd = 0x16,
        Replay = 0x17,
        TimeLimit = 0x18,
        Chat = 0x19,
        HsPlayerEnter = 0x20,
        HsPlayerChange = 0x21,
        HsWatchChange = 0x22
    }
    public enum Reason
    {
        Destory = 0x1,
        Release = 0x2,
        Temporary = 0x4,
        Material = 0x8,
        Summon = 0x10,
        Battle = 0x20,
        Effect = 0x40,
        Cost = 0x80,
        Adjust = 0x100,
        LostTarget = 0x200,
        Rule = 0x400,
        SpSummon = 0x800,
        Dissummon = 0x1000,
        Flip = 0x2000,
        Discard = 0x4000,
        RDamage = 0x8000,
        RRecover = 0x10000,
        Return = 0x20000,
        Fusion = 0x40000,
        Synchro = 0x80000,
        Ritual = 0x100000,
        Xyz = 0x200000,
        Replace = 0x1000000,
        Draw = 0x2000000,
        Redirect = 0x4000000,
        Link = 0x10000000,
    }
    public enum Race
    {
        Warrior = 0x1,
        SpellCaster = 0x2,
        Fairy = 0x4,
        Fiend = 0x8,
        Zombie = 0x10,
        Machine = 0x20,
        Aqua = 0x40,
        Pyro = 0x80,
        Rock = 0x100,
        WindBeast = 0x200,
        Plant = 0x400,
        Insect = 0x800,
        Thunder = 0x1000,
        Dragon = 0x2000,
        Beast = 0x4000,
        BeastWarrior = 0x8000,
        Dinosaur = 0x10000,
        Fish = 0x20000,
        SeaSerpent = 0x40000,
        Reptile = 0x80000,
        Psycho = 0x100000,
        DivineBeast = 0x200000,
        CreatorGod = 0x400000,
        Wyrm = 0x800000,
        Cyberse = 0x1000000
    }
    public enum Query
    {
        Code = 0x1,
        Position = 0x2,
        Alias = 0x4,
        Type = 0x8,
        Level = 0x10,
        Rank = 0x20,
        Attribute = 0x40,
        Race = 0x80,
        Attack = 0x100,
        Defence = 0x200,
        BaseAttack = 0x400,
        BaseDefence = 0x800,
        Reason = 0x1000,
        ReasonCard = 0x2000,
        EquipCard = 0x4000,
        TargetCard = 0x8000,
        OverlayCard = 0x10000,
        Counters = 0x20000,
        Owner = 0x40000,
        IsDisabled = 0x80000,
        IsPublic = 0x100000,
        LScale = 0x200000,
        RScale = 0x400000,
        Link = 0x800000
    }
    public enum OpCode
    {
        Add = 0x40000000,
        Sub = 0x40000001,
        Mul = 0x40000002,
        Div = 0x40000003,
        And = 0x40000004,
        Or = 0x40000005,
        Neg = 0x40000006,
        Not = 0x40000007,
        IsCode = 0x40000100,
        IsSetCard = 0x40000101,
        IsType = 0x40000102,
        IsRace = 0x40000103,
        IsAttribute = 0x40000104,
    }
    public enum DuelPhase
    {
        Draw = 0x01,
        Standby = 0x02,
        Main1 = 0x04,
        BattleStart = 0x08,
        BattleStep = 0x10,
        Damage = 0x20,
        DamageCalculate = 0x40,
        Battle = 0x80,
        Main2 = 0x100,
        End = 0x200
    }
    public enum CardType
    {
        Monster = 0x1,
        Spell = 0x2,
        Trap = 0x4,
        Normal = 0x10,
        Effect = 0x20,
        Fusion = 0x40,
        Ritual = 0x80,
        TrapMonster = 0x100,
        Spirit = 0x200,
        Union = 0x400,
        Dual = 0x800,
        Tuner = 0x1000,
        Synchro = 0x2000,
        Token = 0x4000,
        QuickPlay = 0x10000,
        Continuous = 0x20000,
        Equip = 0x40000,
        Field = 0x80000,
        Counter = 0x100000,
        Flip = 0x200000,
        Toon = 0x400000,
        Xyz = 0x800000,
        Pendulum = 0x1000000,
        SpecialSummon = 0x2000000,
        Link = 0x4000000,
    }
    public enum CardAttribute
    {
        Earth = 0x01,
        Water = 0x02,
        Fire = 0x04,
        Wind = 0x08,
        Light = 0x10,
        Dark = 0x20,
        Divine = 0x40,
    }
    enum GameHint
    {
        Event = 1,
        Message = 2,
        SelectMessage = 3,
        OpSelected = 4,
        Effect = 5,
        Race = 6,
        Attribute = 7,
        Code = 8,
        Number = 9,
        Card = 10,
    }
    enum CardHint
    {
        Turn = 1,
        Card = 2,
        Race = 3,
        Attribute = 4,
        Number = 5,
        DescriptionAdd = 6,
        DescriptionRemove = 7,
    }
    enum PlayerHint
    {
        DescriptionAdd = 6,
        DescriptionRemove = 7,
    }
    enum EDEsc
    {
        Operation = 1,
        Reset = 2,
    }
    public enum PlayerType
    {
        Player1 = 0,
        Player2 = 1,
        Player3 = 2,
        Player4 = 3,
        Player5 = 4,
        Player6 = 5,
        Observer = 7,
    }
    public enum ErrorType
    {
        JoinError = 0x1,
        DeckError = 0x2,
        SideError = 0x3,
        VerionError = 0x4
    }
    public enum DeckError
    {
        LfList = 0x1,
        OcgOnly = 0x2,
        TcgOnly = 0x3,
        UnknownCard = 0x4,
        CardCount = 0x5,
        MainCount = 0x6,
        ExtraCount = 0x7,
        SideCount = 0x8
    }
    public enum PlayerState
    {
        Observe = 0x8,
        Ready = 0x9,
        NotReady = 0xA,
        Leave = 0xB
    }
}