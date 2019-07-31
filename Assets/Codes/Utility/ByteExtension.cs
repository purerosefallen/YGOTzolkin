using System;
using System.IO;
using System.Text;


namespace YGOTzolkin.Utility
{
    static class ByteExtension
    {
        public static byte[] StringToUnicode(string source, int bytesLength)
        {
            byte[] destination = new byte[bytesLength];
            byte[] sourceBytes = Encoding.Unicode.GetBytes(source);
            Array.Copy(sourceBytes, destination, sourceBytes.Length > bytesLength ? bytesLength : sourceBytes.Length);
            return destination;
        }

        public static string ByteToString(byte[] bytes)
        {
            StringBuilder rawData = new StringBuilder();
            for (int i = 0; i < bytes.Length; ++i)
            {
                rawData.Append(bytes[i].ToString("X2")).Append(" ");
            }
            return rawData.ToString();
        }

        public static string ReadUnicode(this BinaryReader reader, int byteCount)
        {
            byte[] unicode = reader.ReadBytes(byteCount);
            string text = Encoding.Unicode.GetString(unicode);
            text = text.Substring(0, text.IndexOf('\0'));
            return text;
        }

        public static Coordinate ReadCoordinate(this BinaryReader reader)
        {
            return new Coordinate
            {
                Controller = GameInfo.Instance.LocalPlayer(reader.ReadByte()),
                Location = reader.ReadByte(),
                Sequence = reader.ReadByte(),
            };
        }
    }
}
