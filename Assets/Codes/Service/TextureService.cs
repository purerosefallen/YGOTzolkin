using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YGOTzolkin.Utility;

namespace YGOTzolkin.Service
{
    public static class TextureService
    {
        public static List<Texture2D> ForbiddenIcons { get; private set; }
        public static Texture2D Cover2 { get; private set; }
        public static Texture2D Cover { get; private set; }

        private static Texture2D unknownCard;
        private static readonly LRUCache<uint, Texture2D> cardTexCache = new LRUCache<uint, Texture2D>(3000);

        public static Texture2D ReadTexture(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            try
            {
                FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                byte[] data = new byte[file.Length];
                file.Read(data, 0, (int)file.Length);
                file.Close();
                Texture2D pic = new Texture2D(484, 700);
                pic.LoadImage(data);
                return pic;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Texture2D GetCardTexture(uint code)
        {
            if (code <= 0)
            {
                return unknownCard;
            }
            Texture2D texture = cardTexCache.Get(code);
            if (texture == null)
            {
                string path = string.Format("{0}{1}.jpg", Config.PicturePath, code);
                if (!File.Exists(path))
                {
                    texture = unknownCard;
                }
                else
                {
                    texture = ReadTexture(path);
                    cardTexCache.Put(code, texture);
                }
            }
            return texture;
        }

        public static bool LoadTextures()
        {
            ForbiddenIcons = new List<Texture2D>
            {
                ReadTexture(string.Format("textures/fl{0}.png", 0)),
                ReadTexture(string.Format("textures/fl{0}.png", 1)),
                ReadTexture(string.Format("textures/fl{0}.png", 2)),
                ReadTexture(string.Format("textures/fl{0}.png", 3)),
            };
            unknownCard = ReadTexture("textures/unknown.jpg");
            Cover2 = ReadTexture("textures/cover2.jpg");
            Cover = ReadTexture("textures/cover.jpg");
            return true;
        }
    }
}
