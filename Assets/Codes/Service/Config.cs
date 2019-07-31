using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YGOTzolkin.Service
{
    public static class Config
    {
        public static readonly ushort YGOVersion = 0x134B;
        public static readonly byte DefaultMasterRule = 4;

        public static string DeckPath { get; private set; }
        public static string TexturePath { get; private set; }
        public static string ReplayPath { get; private set; }
        public static string CDBPath { get; private set; }
        public static string LfListPath { get; private set; }
        public static string PicturePath { get; private set; }


        private static readonly string ConfigFileName = "tzl.conf";
        private static readonly char Separator = '=';
        private static readonly char Comment = '#';
        private static Dictionary<string, string> fields;
        private static Dictionary<string, int> integerCache;
        private static Dictionary<string, bool> booleanCache;

        public static void InitConfig()
        {
            DeckPath = "deck/";
            TexturePath = "textures/";
            ReplayPath = "replay/";
            PicturePath = "pics/";
            CDBPath = "cards.cdb";
            LfListPath = "lflist.conf";
            if(!Directory.Exists(DeckPath))
            {
                Directory.CreateDirectory(DeckPath);
            }
            if(!Directory.Exists(ReplayPath))
            {
                Directory.CreateDirectory(ReplayPath);
            }

            integerCache = new Dictionary<string, int>();
            booleanCache = new Dictionary<string, bool>();
            fields = new Dictionary<string, string>();
            try
            {
                StreamReader reader = new StreamReader(ConfigFileName);
                int lineNumber = 0;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine().Trim();
                    ++lineNumber;

                    if (line.Length == 0 || line[0] == Comment || line[0] == '\n')
                    {
                        continue;
                    }

                    int position = line.IndexOf(Separator);

                    if (position == -1)
                    {
                        Debug.LogAssertion("Invalid configuration file: no key/value separator line " + lineNumber);
                        continue;
                    }

                    string key = line.Substring(0, position).Trim();
                    string value = line.Substring(position + 1).Trim();

                    if (fields.ContainsKey(key))
                    {
                        Debug.LogAssertion("Invalid configuration file: duplicate key '" + key + "' line " + lineNumber);
                        continue;
                    }

                    fields.Add(key, value);
                }
            }
            catch (Exception e)
            {
                Debug.LogAssertion(e.Message + e.GetType() + e.StackTrace);
            }
        }

        public static void Save()
        {
            FileStream file;
            if (File.Exists(ConfigFileName))
            {
                file = new FileStream(ConfigFileName, FileMode.Truncate, FileAccess.Write);
            }
            else
            {
                file = new FileStream(ConfigFileName, FileMode.Create, FileAccess.Write);
            }
            StreamWriter writer = new StreamWriter(file);
            foreach (var pair in fields)
            {
                writer.Write(string.Format("{0}={1}\n", pair.Key, pair.Value));
            }
            writer.Close();
            file.Close();
        }

        public static string GetString(string key, string defaultValue = null)
        {
            if (fields.TryGetValue(key, out string val))
            {
                return val;
            }
            else
            {
                fields.Add(key, defaultValue.ToString());
            }

            return defaultValue;
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            // Use a cache to prevent doing the string to int conversion over and over
            if (integerCache.TryGetValue(key, out int val))
            {
                return val;
            }

            int value = defaultValue;
            if (fields.TryGetValue(key, out string strVal))
            {
                if (strVal.StartsWith("0x"))
                {
                    value = Convert.ToInt32(strVal, 16);
                }
                else
                {
                    value = Convert.ToInt32(strVal);
                }
            }
            else
            {
                fields.Add(key, defaultValue.ToString());
            }
            integerCache.Add(key, value);
            return value;
        }

        public static uint GetUInt(string key, uint defaultValue = 0)
        {
            return (uint)GetInt(key, (int)defaultValue);
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            // Same here, prevent from redoing the string to bool conversion
            if (booleanCache.TryGetValue(key, out bool val))
            {
                return val;
            }

            bool value = defaultValue;
            if (fields.TryGetValue(key, out string strVal))
            {
                value = Convert.ToBoolean(strVal);
            }
            else
            {
                fields.Add(key, defaultValue.ToString());
            }
            booleanCache.Add(key, value);
            return value;
        }

        internal static void Set(string key, string value)
        {
            if (fields.ContainsKey(key))
            {
                fields[key] = value;
            }
            else
            {
                fields.Add(key, value);
            }

            if (booleanCache.ContainsKey(key))
            {
                booleanCache.Remove(key);
            }
            if (integerCache.ContainsKey(key))
            {
                integerCache.Remove(key);
            }
        }
    }
}
