using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using YGOTzolkin.Service;

namespace YGOTzolkin.YGOModel
{
    public class CardData
    {
        public uint Code { get; set; }
        public uint OTExclusive { get; set; }
        public uint Alias { get; set; }
        public ulong Series { get; set; }
        public uint Type { get; set; }
        public uint Level { get; set; }
        public uint LScale { get; set; }
        public uint RScale { get; set; }
        public uint Attribute { get; set; }
        public uint Race { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public uint LinkMarker { get; set; }
        public uint Category { get; set; }
        public string Name { get; set; }
        public string Descrition { get; set; }
        public List<string> Options { get; set; }

        public string Details { get; set; }
        public string Summary { get; set; }
        public string PanelBrief { get; set; }
        public string TotalInfo { get; set; }


        public CardData(IDataRecord reader)
        {
            Code = (uint)reader.GetInt32(0);
            OTExclusive = (uint)reader.GetInt32(1);
            Alias = (uint)reader.GetInt32(2);
            Series = (ulong)reader.GetInt64(3);
            Type = (uint)reader.GetInt32(4);
            Attack = reader.GetInt32(5);
            Defense = reader.GetInt32(6);
            BaseAttack = Attack;
            if ((Type & (uint)CardType.Link) > 0)
            {
                LinkMarker = (uint)Defense;
                Defense = 0;
            }
            else
            {
                LinkMarker = 0;
            }
            BaseDefense = Defense;
            long rawLevel = reader.GetInt64(7);
            Level = (uint)rawLevel & 0xff;
            LScale = (uint)((rawLevel >> 0x18) & 0xff);
            RScale = (uint)((rawLevel >> 0x10) & 0xff);
            Race = (uint)reader.GetInt64(8);
            Attribute = (uint)reader.GetInt32(9);
            Category = (uint)reader.GetInt32(10);
            Name = reader.GetString(12);
            Descrition = reader.GetString(13);
            Options = new List<string>();
            for (int i = 0; i < 16; ++i)
            {
                Options.Add(reader.GetString(14 + i));
            }
            FormatDetails();
            FormatSummary();
            FormatBrief();
            TotalInfo = Details + "\n" + Descrition;
        }

        public CardData()
        {
            Reset();
        }

        public void Reset()
        {
            Code = OTExclusive = Alias = 0;
            Series = Category = 0; ;
            Type = Attribute = Race = LinkMarker = 0;
            Level = LScale = RScale = 0;
            Attack = Defense = BaseAttack = BaseDefense = -1;
            Name = null;
            Descrition = null;
        }

        private void FormatDetails()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<b>").Append(Name).Append("</b>\n[").Append(DataService.FormatType(Type)).Append("]");
            if ((Type & (uint)CardType.Monster) > 0)
            {
                builder.Append(string.Format("  {0}|{1}\n", DataService.FormatRace(Race), DataService.FormatAttribute(Attribute)));
                if ((Type & (uint)CardType.Link) > 0)
                {
                    builder.Append(string.Format("[LINK-{0}] {1}/- [{2}]\n", Level, Attack, DataService.FormatLinkMarker(LinkMarker)));
                }
                else
                {
                    builder.Append(string.Format("[{0}{1}] {2}/{3}\n",
                        (Type & (uint)CardType.Xyz) > 0 ? "\u2606" : "\u2605", Level, Attack >= 0 ? Attack.ToString() : "?", Defense >= 0 ? Defense.ToString() : "?"));
                }
            }
            if (Series > 0)
            {
                builder.Append(DataService.SysString(1329)).Append(DataService.FormatSeries(Series));
            }
            Details = builder.ToString();
        }

        private void FormatBrief()
        {
            StringBuilder builder = new StringBuilder(Name);
            if ((Type & (uint)CardType.Monster) > 0)
            {
                builder.Append("\n").Append(Attack).Append("/");
                if ((Type & (int)CardType.Link) > 0)
                {
                    builder.Append("-\n").Append("LINK-").Append(Level);
                }
                else
                {
                    builder.Append(Defense).Append("\n");
                    if ((Type & (uint)CardType.Xyz) > 0)
                    {
                        builder.Append("\u2606").Append(Level);
                    }
                    else
                    {
                        builder.Append("\u2605").Append(Level);
                    }
                }
            }
            PanelBrief = builder.ToString();
        }

        private void FormatSummary()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<color=#ffff4cff><b>").Append(Name).Append("</b></color>").Append("\n");
            if ((Type & (uint)CardType.Monster) > 0)
            {
                builder.Append("<color=#b7ff7dff>");
                uint type = Type - 0x1;
                builder.Append(DataService.FormatType(type)).Append(" ");
                builder.Append(DataService.FormatRace(Race)).Append("/");
                builder.Append(DataService.FormatAttribute(Attribute)).Append("\n");
                if ((Type & (uint)CardType.Link) > 0)
                {
                    builder.Append("LINK-").Append(Level).Append(" ");
                }
                else if ((Type & (uint)CardType.Xyz) > 0)
                {
                    builder.Append("\u2606").Append(Level).Append(" ");
                }
                else
                {
                    builder.Append("\u2605").Append(Level).Append(" ");
                }
                if (Attack >= 0)
                {
                    builder.Append(Attack).Append("/");
                }
                else
                {
                    builder.Append("?").Append("/");
                }
                if (Defense >= 0)
                {
                    if ((Type & (uint)CardType.Link) > 0)
                    {
                        builder.Append("-");
                    }
                    else
                    {
                        builder.Append(Defense);
                    }
                }
                else
                {
                    builder.Append("?");
                }
                builder.Append("</color>");
            }
            else
            {
                builder.Append(DataService.FormatType(Type));
            }
            if (OTExclusive == 1)
            {
                builder.Append(" [OCG]");
            }
            else if (OTExclusive == 2)
            {
                builder.Append(" [TCG]");
            }
            Summary = builder.ToString();
        }

        public bool IsExtra()
        {
            return (Type & (uint)CardType.Fusion) > 0 || (Type & (uint)CardType.Synchro) > 0
                || (Type & (uint)CardType.Xyz) > 0 || (Type & (uint)CardType.Link) > 0;
        }

        public override string ToString()
        {
            return Details;
        }

        //something wrong
        public static int CompareByName(CardData left, CardData right)
        {
            int result = string.Compare(left.Name, right.Name, StringComparison.Ordinal);
            if (result != 0)
            {
                return result;
            }
            else
            {
                return left.Code > right.Code ? 1 : -1;
            }
        }

        public static int CompareByStars(CardData left, CardData right)
        {
            uint ltype = left.Type & 0x7;
            uint rtype = right.Type & 0x7;
            if (ltype != rtype)
            {
                return ltype < rtype ? -1 : 1;
            }
            if (ltype == 1)
            {
                uint type1 = ((left.Type & 0x48020c0) > 0) ? (left.Type & 0x48020c1) : (left.Type & 0x31);
                uint type2 = ((right.Type & 0x48020c0) > 0) ? (right.Type & 0x48020c1) : (right.Type & 0x31);
                if (type1 != type2)
                {
                    return type1 < type2 ? -1 : 1;
                }
                if (left.Level != right.Level)
                {
                    return left.Level < right.Level ? 1 : -1;
                }
                if (left.Attack != right.Attack)
                {
                    return left.Attack < right.Attack ? 1 : -1;
                }
                if (left.Defense != right.Defense)
                {
                    return left.Defense < right.Defense ? 1 : -1;
                }
            }
            else
            {
                ltype = left.Type & 0xfffffff8;
                rtype = right.Type & 0xfffffff8;
                if (ltype != rtype)
                {
                    return ltype < rtype ? -1 : 1;
                }
            }
            return left.Code < right.Code ? -1 : 1;
        }

        public static int CompareByAttack(CardData left, CardData right)
        {
            uint ltype = left.Type & 0x7;
            uint rtype = right.Type & 0x7;
            if (ltype != rtype)
            {
                return ltype < rtype ? -1 : 1;
            }
            if (ltype == 1)
            {
                uint type1 = ((left.Type & 0x48020c0) > 0) ? (left.Type & 0x48020c1) : (left.Type & 0x31);
                uint type2 = ((right.Type & 0x48020c0) > 0) ? (right.Type & 0x48020c1) : (right.Type & 0x31);
                if (left.Attack != right.Attack)
                {
                    return left.Attack > right.Attack ? -1 : 1;
                }
                if (left.Defense != right.Defense)
                {
                    return left.Defense > right.Defense ? -1 : 1;
                }
                if (left.Level != right.Level)
                {
                    return left.Level > right.Level ? -1 : 1;
                }
                if (type1 != type2)
                {
                    return type1 > type2 ? 1 : -1;
                }
            }
            else
            {
                ltype = left.Type & 0xfffffff8;
                rtype = right.Type & 0xfffffff8;
                if (ltype != rtype)
                {
                    return ltype < rtype ? -1 : 1;
                }
            }
            return left.Code < right.Code ? -1 : 1;
        }

        public static int CompareByDefense(CardData left, CardData right)
        {
            uint ltype = left.Type & 0x7;
            uint rtype = right.Type & 0x7;
            if (ltype != rtype)
            {
                return ltype < rtype ? -1 : 1;
            }
            if (ltype == 1)
            {
                uint type1 = ((left.Type & 0x48020c0) > 0) ? (left.Type & 0x48020c1) : (left.Type & 0x31);
                uint type2 = ((right.Type & 0x48020c0) > 0) ? (right.Type & 0x48020c1) : (right.Type & 0x31);
                if (left.Defense != right.Defense)
                {
                    return left.Defense > right.Defense ? -1 : 1;
                }
                if (left.Attack != right.Attack)
                {
                    return left.Attack > right.Attack ? -1 : 1;
                }
                if (left.Level != right.Level)
                {
                    return left.Level > right.Level ? -1 : 1;
                }
                if (type1 != type2)
                {
                    return type1 > type2 ? 1 : -1;
                }
            }
            else
            {
                ltype = left.Type & 0xfffffff8;
                rtype = right.Type & 0xfffffff8;
                if (ltype != rtype)
                {
                    return ltype < rtype ? -1 : 1;
                }
            }
            return left.Code < right.Code ? -1 : 1;
        }
    }
}
