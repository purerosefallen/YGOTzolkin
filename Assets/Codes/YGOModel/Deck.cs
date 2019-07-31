using System.Collections.Generic;
using System.IO;

namespace YGOTzolkin.YGOModel
{
    public class Deck
    {
        public List<uint> Main { get; private set; }
        public List<uint> Extra { get; private set; }
        public List<uint> Side { get; private set; }
        public List<List<uint>> AllCards { get; private set; }

        public int CardCount
        {
            get
            {
                return Main.Count + Extra.Count + Side.Count;
            }
        }

        public Deck()
        {
            Main = new List<uint>();
            Extra = new List<uint>();
            Side = new List<uint>();
            AllCards = new List<List<uint>>
            {
                Main,
                Extra,
                Side
            };
        }

        public void Clear()
        {
            Main.Clear();
            Extra.Clear();
            Side.Clear();
        }

        public byte[] ToArray()
        {
            int dct = CardCount;
            MemoryStream stream = new MemoryStream(11 + dct * 4);
            BinaryWriter writer = new BinaryWriter(stream);
       
            ushort len = (ushort)(9 + dct * 4);
            writer.Write(len);
            writer.Write((byte)CToSMessage.UpdateDeck);
            writer.Write(Main.Count + Extra.Count);
            writer.Write(Side.Count);
            for (int i = 0; i < Main.Count; i++)
            {
                writer.Write(Main[i]);
            }
            for (int i = 0; i < Extra.Count; i++)
            {
                writer.Write(Extra[i]);
            }
            for (int i = 0; i < Side.Count; i++)
            {
                writer.Write(Side[i]);
            }
            return stream.ToArray();
        }
    }
}
