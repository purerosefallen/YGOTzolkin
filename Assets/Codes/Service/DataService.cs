using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using UnityEngine;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.Service
{
    public static class DataService
    {
        private static readonly Dictionary<uint, CardData> cardDictionary;
        private static readonly Dictionary<uint, string> systemStrings;
        private static readonly Dictionary<uint, string> victoryStrings;
        private static readonly Dictionary<uint, string> counterStrings;
        private static readonly Dictionary<uint, string> seriesStrings;
        private static readonly Dictionary<uint, string> tzlStrings;

        private static readonly string unknownString;
        private static CardData unknownCard;

        static DataService()
        {
            unknownString = "[unknown]";
            cardDictionary = new Dictionary<uint, CardData>();
            systemStrings = new Dictionary<uint, string>();
            victoryStrings = new Dictionary<uint, string>();
            counterStrings = new Dictionary<uint, string>();
            seriesStrings = new Dictionary<uint, string>();
            tzlStrings = new Dictionary<uint, string>();
        }

        public static string SysString(uint code)
        {
            if (systemStrings.TryGetValue(code, out string value))
            {
                return value;
            }
            else
            {
                return unknownString;
            }
        }

        public static string CounterName(uint code)
        {
            if (counterStrings.TryGetValue(code, out string value))
            {
                return value;
            }
            else
            {
                return unknownString;
            }
        }

        public static string VictoryString(uint code)
        {
            if (victoryStrings.TryGetValue(code, out string value))
            {
                return value;
            }
            else
            {
                return unknownString;
            }
        }

        public static string SeriesString(uint code)
        {
            if (seriesStrings.TryGetValue(code, out string value))
            {
                return value;
            }
            else
            {
                return unknownString;
            }
        }

        public static string GetDescription(uint code)
        {
            if (code < 10000)
            {
                return SysString(code);
            }
            uint card = code >> 4;
            uint oft = code & 0xf;
            if (cardDictionary.TryGetValue(card, out CardData data))
            {
                return data.Options[(int)oft];
            }
            else
            {
                return unknownString;
            }
        }

        public static string TzlString(uint code)
        {
            if (tzlStrings.TryGetValue(code, out string value))
            {
                return value;
            }
            else
            {
                return unknownString;
            }
        }

        public static CardData GetCardData(uint code)
        {
            if (cardDictionary.TryGetValue(code, out CardData data))
            {
                return data;
            }
            if (unknownCard == null)
            {
                unknownCard = new CardData
                {
                    Name = "unknown",
                    Code = 0
                };
            }
            return unknownCard;
        }

        public static string GetName(uint code)
        {
            if (cardDictionary.TryGetValue(code, out CardData data))
            {
                return data.Name;
            }
            return unknownString;
        }

        public static bool LoadData()
        {
            return LoadStrings("strings.conf") && LoadCDB() && LoadTzolkin();
        }

        private static bool LoadCDB()
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=" + Config.CDBPath))
            {
                connection.Open();
                using (IDbCommand command = new SqliteCommand("SELECT datas.*, texts.* FROM datas,texts WHERE datas.id=texts.id;", connection))
                {
                    IDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CardData card = new CardData(reader);
                        if (!cardDictionary.ContainsKey(card.Code))
                        {
                            cardDictionary.Add(card.Code, card);
                        }
                        else
                        {
                            Debug.LogAssertion("Duplicate code!");
                        }
                    }
                    reader.Close();
                }
            }
            return true;
        }

        private static bool LoadStrings(string path)
        {
            try
            {
                FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(file, Encoding.UTF8);
                string line;
                string[] content;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith("!"))
                    {
                        continue;
                    }
                    content = line.Split(new string[] { " ", "	" }, StringSplitOptions.RemoveEmptyEntries);
                    if (content[0].Equals("!system"))
                    {
                        systemStrings.Add(uint.Parse(content[1]), FilterPlaceholders(content[2]));
                    }
                    else if (content[0].Equals("!victory"))
                    {
                        victoryStrings.Add(Convert.ToUInt32(content[1], 16), FilterPlaceholders(content[2]));
                    }
                    else if (content[0].Equals("!counter"))
                    {
                        counterStrings.Add(Convert.ToUInt32(content[1], 16), content[2]);
                    }
                    else if (content[0].Equals("!setname"))
                    {
                        seriesStrings.Add(Convert.ToUInt32(content[1], 16), content[2]);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static bool LoadTzolkin()
        {
            string path = "tzlstrings.conf";
            try
            {
                FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(file, Encoding.UTF8);
                string line;
                string[] content;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith("!"))
                    {
                        continue;
                    }
                    content = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (content[0].Equals("!system"))
                    {
                        tzlStrings.Add(uint.Parse(content[1]), content[2]);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string FormatType(uint type)
        {
            if (type == 0)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            uint filter = 1, i = 1050;
            for (; filter != 0x8000000; filter <<= 1, ++i)
            {
                if ((type & filter) > 0)
                {
                    builder.Append(SysString(i)).Append('|');
                }
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 1, 1);
            }
            return builder.ToString();
        }

        public static string FormatRace(uint race)
        {
            if (race == 0)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            uint filter = 1, i = 1020;
            for (; filter != 0x2000000; filter <<= 1, ++i)
            {
                if ((race & filter) > 0)
                {
                    builder.Append(SysString(i)).Append('/');
                }
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 1, 1);
            }
            return builder.ToString();
        }

        public static string FormatAttribute(uint attribute)
        {
            if (attribute == 0)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            uint filter = 1, i = 1010;
            for (; filter != 0x80; filter <<= 1, ++i)
            {
                if ((attribute & filter) > 0)
                {
                    builder.Append(SysString(i)).Append('/');
                }
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 1, 1);
            }
            return builder.ToString();
        }

        public static string FormatLinkMarker(uint marker)
        {
            if (marker == 0)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            if ((marker & (int)LinkMarker.TopLeft) > 0)
            {
                builder.Append("\u2196");
            }
            if ((marker & (int)LinkMarker.Top) > 0)
            {
                builder.Append("\u2191");
            }
            if ((marker & (int)LinkMarker.TopRight) > 0)
            {
                builder.Append("\u2197");
            }
            if ((marker & (int)LinkMarker.Left) > 0)
            {
                builder.Append("\u2190");
            }
            if ((marker & (int)LinkMarker.Right) > 0)
            {
                builder.Append("\u2192");
            }
            if ((marker & (int)LinkMarker.BottomLeft) > 0)
            {
                builder.Append("\u2199");
            }
            if ((marker & (int)LinkMarker.Bottom) > 0)
            {
                builder.Append("\u2193");
            }
            if ((marker & (int)LinkMarker.BottomRight) > 0)
            {
                builder.Append("\u2198");
            }
            return builder.ToString();
        }

        public static string FormatLocation(int location, int sequence)
        {
            if (location == 0x8)
            {
                if (sequence < 5)
                {
                    return SysString(1003);
                }
                else if (sequence == 5)
                {
                    return SysString(1008);
                }
                else
                {
                    return SysString(1009);
                }
            }
            uint filter = 1, i = 1000;
            for (; filter != 0x100 && filter != location; filter <<= 1)
            {
                ++i;
            }
            if (filter == location)
            {
                return SysString(i);
            }
            else
            {
                return unknownString;
            }
        }

        public static string FormatSeries(ulong series)
        {
            if (series == 0)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 4; ++i)
            {
                uint key = Convert.ToUInt32((series >> (i * 16)) & 0xffff);
                if (seriesStrings.TryGetValue(key, out string val))
                {
                    builder.Append(val).Append('|');
                }
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 1, 1);
            }
            return builder.ToString();
        }

        public static List<CardData> Search(string str, List<int> opCodes)
        {
            List<CardData> results = new List<CardData>();
            if (str == null)
            {
                return results;
            }
            if (uint.TryParse(str, out uint code))
            {
                if (cardDictionary.TryGetValue(code, out CardData data) && IsDeclarable(data, opCodes))
                {
                    results.Add(data);
                    return results;
                }
            }
            foreach (var data in cardDictionary.Values)
            {
                if (data.Name.Contains(str) && IsDeclarable(data, opCodes))
                {
                    if (data.Name == str)
                    {
                        results.Insert(0, data);
                    }
                    else
                    {
                        results.Add(data);
                    }
                }
            }
            return results;
        }

        public static List<CardData> Search(CardData target, ForbiddenList flist = null, int qualification = 3)
        {
            List<CardData> results = new List<CardData>();
            if (target.Name != null && uint.TryParse(target.Name, out uint tempcode))
            {
                if (cardDictionary.TryGetValue(tempcode, out CardData data))
                {
                    results.Add(data);
                    return results;
                }
            }
            foreach (var data in cardDictionary.Values)
            {
                if ((data.Type & (uint)CardType.Token) > 0)
                {
                    continue;
                }
                if (flist != null && qualification < 3)
                {
                    if (flist.Query(data) != qualification)
                    {
                        continue;
                    }
                }
                if (target.Type > 0)
                {
                    if ((target.Type & 0x1) > 0)
                    {
                        if ((target.Type & data.Type) != target.Type)
                        {
                            continue;
                        }
                        if ((target.Race != 0 && (target.Race & data.Race) != target.Race) ||
                            (target.Attribute != 0 && (target.Attribute & data.Attribute) == 0))
                        {
                            continue;
                        }
                        if ((target.Attack > 0 && target.Attack != data.Attack))
                        {
                            continue;
                        }
                        if ((target.Type & (uint)CardType.Link) == 0 &&
                            target.Defense > 0 && target.Defense != data.Defense)
                        {
                            continue;
                        }
                        if ((target.Level > 0 && target.Level != data.Level) ||
                            (target.LScale > 0 && target.LScale != data.LScale))
                        {
                            continue;
                        }
                        if (target.LinkMarker != 0 && (data.LinkMarker & target.LinkMarker) != target.LinkMarker)
                        {
                            continue;
                        }
                    }
                    else if ((target.Type & 0x2) > 0)
                    {
                        if (target.Type == 0x2)
                        {
                            if ((data.Type & 0x2) == 0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if ((data.Type & target.Type) != data.Type)
                            {
                                continue;
                            }
                            if ((target.Type & (uint)CardType.Normal) == 0)
                            {
                                if (data.Type == 0x2)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
                else if ((target.Attribute & 0x4) > 0)
                {
                    if (target.Attribute == 0x4)
                    {
                        if ((data.Type & 0x4) == 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if ((data.Type & target.Attribute) != data.Type)
                        {
                            continue;
                        }
                        if ((target.Attribute & (uint)CardType.Normal) == 0)
                        {
                            if (data.Type == 0x4)
                            {
                                continue;
                            }
                        }
                    }
                }
                if (target.Name != null)
                {
                    if (!data.Name.Contains(target.Name) && !data.Descrition.Contains(target.Name))
                    {
                        continue;
                    }
                }
                if (target.Category > 0 && !((target.Category & data.Category) > 0))
                {
                    continue;
                }
                if (target.OTExclusive > 0 && target.OTExclusive != data.OTExclusive)
                {
                    continue;
                }
                results.Add(data);
            }
            return results;
        }

        private static string FilterPlaceholders(string content)
        {
            int counter = 0;
            string[] parts = content.Split('%');
            if (parts.Length > 1)
            {
                StringBuilder builder = new StringBuilder(parts[0]);
                for (int i = 1; i < parts.Length; ++i)
                {
                    switch (parts[i][0])
                    {
                        case 'l':
                            builder.Append("{").Append(counter).Append("}").Append(parts[i].Replace("ls", ""));
                            counter++;
                            break;
                        case 'd':
                            builder.Append("{").Append(counter).Append("}").Append(parts[i].Replace("d", ""));
                            counter++;
                            break;
                        default: break;
                    }
                }
                return builder.ToString();
            }
            return content;
        }

        private static bool IsDeclarable(CardData data, List<int> opCodes)
        {
            Stack<int> stack = new Stack<int>();
            foreach (var op in opCodes)
            {
                switch (op)
                {
                    case (int)OpCode.Add:
                        if (stack.Count >= 2)
                        {
                            int rhs = stack.Pop();
                            int lhs = stack.Pop();
                            stack.Push(rhs + lhs);
                        }
                        break;
                    case (int)OpCode.Sub:
                        if (stack.Count >= 2)
                        {
                            int rhs = stack.Pop();
                            int lhs = stack.Pop();
                            stack.Push(rhs - lhs);
                        }
                        break;
                    case (int)OpCode.Mul:
                        if (stack.Count >= 2)
                        {
                            int rhs = stack.Pop();
                            int lhs = stack.Pop();
                            stack.Push(rhs * lhs);
                        }
                        break;
                    case (int)OpCode.Div:
                        if (stack.Count >= 2)
                        {
                            int rhs = stack.Pop();
                            int lhs = stack.Pop();
                            stack.Push(rhs / lhs);
                        }
                        break;
                    case (int)OpCode.And:
                        if (stack.Count >= 2)
                        {
                            int rhs = stack.Pop();
                            int lhs = stack.Pop();
                            stack.Push((rhs > 0 && lhs > 0) ? 1 : 0);
                        }
                        break;
                    case (int)OpCode.Or:
                        if (stack.Count >= 2)
                        {
                            int rhs = stack.Pop();
                            int lhs = stack.Pop();
                            stack.Push((rhs > 0 || lhs > 0) ? 1 : 0);
                        }
                        break;
                    case (int)OpCode.Neg:
                        if (stack.Count >= 1)
                        {
                            int rhs = stack.Pop();
                            stack.Push(-rhs);
                        }
                        break;
                    case (int)OpCode.Not:
                        if (stack.Count >= 1)
                        {
                            int rhs = stack.Pop();
                            stack.Push(rhs > 0 ? 0 : 1);
                        }
                        break;
                    case (int)OpCode.IsCode:
                        if (stack.Count >= 1)
                        {
                            int rhs = stack.Pop();
                            stack.Push((uint)rhs == data.Code ? 1 : 0);
                        }
                        break;
                    case (int)OpCode.IsSetCard:
                        if (stack.Count >= 1)
                        {
                            ulong val = (ulong)stack.Pop();
                            ulong settype = val & 0xfff;
                            ulong setsubtype = val & 0xf000;
                            ulong sc = data.Series;
                            bool res = false;
                            while (sc > 0)
                            {
                                if ((sc & 0xfff) == settype && (sc & 0xf000 & setsubtype) == setsubtype)
                                    res = true;
                                sc >>= 16;
                            }
                            stack.Push(res ? 1 : 0);
                        }
                        break;
                    case (int)OpCode.IsType:
                        if (stack.Count >= 1)
                        {
                            int rhs = stack.Pop();
                            stack.Push(((uint)rhs & data.Type) > 0 ? 1 : 0);
                        }
                        break;
                    case (int)OpCode.IsRace:
                        if (stack.Count >= 1)
                        {
                            int rhs = stack.Pop();
                            stack.Push(((uint)rhs & data.Race) > 0 ? 1 : 0);
                        }
                        break;
                    case (int)OpCode.IsAttribute:
                        if (stack.Count >= 1)
                        {
                            int rhs = stack.Pop();
                            stack.Push(((uint)rhs & data.Attribute) > 0 ? 1 : 0);
                        }
                        break;
                    default:
                        stack.Push(op);
                        break;
                }
            }
            if (stack.Count != 1 || stack.Peek() == 0)
            {
                return false;
            }
            return data.Code == 78734254 || data.Code == 13857930
                || (!(data.Alias > 0) && (data.Type & ((int)CardType.Monster + (int)CardType.Token)) != ((int)CardType.Monster + (int)CardType.Token));
        }
    }
}
