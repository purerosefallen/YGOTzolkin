using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.Service
{
    public static class DeckService
    {
        public static Deck CurrentDeck = new Deck();

        public static List<ForbiddenList> ForbiddenLists { get; private set; }

        public static bool Save(string deckPath)
        {
            StringBuilder ydkString = new StringBuilder();
            ydkString.Append("#created by YGOTzolkin\r\n#main\r\n");
            for (int i = 0; i < CurrentDeck.Main.Count; ++i)
            {
                ydkString.Append(CurrentDeck.Main[i].ToString() + "\r\n");
            }
            ydkString.Append("#extra\r\n");
            for (int i = 0; i < CurrentDeck.Extra.Count; ++i)
            {
                ydkString.Append(CurrentDeck.Extra[i].ToString() + "\r\n");
            }
            ydkString.Append("!side\r\n");
            for (int i = 0; i < CurrentDeck.Side.Count; ++i)
            {
                ydkString.Append(CurrentDeck.Side[i].ToString() + "\r\n");
            }
            try
            {
                File.WriteAllText(deckPath, ydkString.ToString(), Encoding.UTF8);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static List<string> GetAllNames()
        {
            var fileInfos = new DirectoryInfo(Config.DeckPath).GetFiles("*.ydk", SearchOption.TopDirectoryOnly);
            List<string> decks = new List<string>(fileInfos.Length);
            for (int i = 0; i < fileInfos.Length; ++i)
            {
                decks.Add(Path.GetFileNameWithoutExtension(fileInfos[i].FullName));
            }
            return decks;
        }

        public static void Load(string deckPath)
        {
            CurrentDeck.Clear();
            if (!File.Exists(deckPath))
            {
                return;
            }
            FileStream file = new FileStream(deckPath, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(file, System.Text.Encoding.UTF8);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!line.Equals("#main", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Equals("#extra", StringComparison.OrdinalIgnoreCase))
                    {
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Equals("!side"))
                            {
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (uint.TryParse(line, out uint scode))
                                    {
                                        CurrentDeck.Side.Add(scode);
                                    }
                                }
                                break;
                            }
                            if (uint.TryParse(line, out uint ecode))
                            {
                                CurrentDeck.Extra.Add(ecode);
                            }
                        }
                        break;
                    }
                    if (uint.TryParse(line, out uint code))
                    {
                        CurrentDeck.Main.Add(code);
                    }
                }
            }
            file.Close();
            reader.Close();
        }

        public static void LoadForbiddenLists()
        {
            ForbiddenLists = new List<ForbiddenList>();
            try
            {
                ForbiddenList current = null;
                StreamReader reader = new StreamReader(Config.LfListPath);
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    {
                        continue;
                    }
                    if (line.StartsWith("!"))
                    {
                        current = new ForbiddenList(line.Substring(1, line.Length - 1));
                        ForbiddenLists.Add(current);
                        continue;
                    }
                    if (!line.Contains(" ") || current == null)
                    {
                        continue;
                    }
                    string[] data = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (data?.Length > 1 && uint.TryParse(data[0], out uint code) && int.TryParse(data[1], out int qualf))
                    {
                        current.Add(code, qualf);
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogAssertion(e.Message + e.GetType() + e.StackTrace);
            }
            ForbiddenLists.Add(new ForbiddenList("N/A"));
        }
    }
}
