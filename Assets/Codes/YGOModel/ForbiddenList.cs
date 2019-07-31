using System.Collections.Generic;

namespace YGOTzolkin.YGOModel
{
    public class ForbiddenList
    {
        public List<uint> Forbidden { get; private set; }
        public List<uint> Limited { get; private set; }
        public List<uint> SemiLimited { get; private set; }
        public uint Hash { get; private set; }
        public string Name { get; private set; }

        public ForbiddenList(string name)
        {
            Forbidden = new List<uint>();
            Limited = new List<uint>();
            SemiLimited = new List<uint>();
            Name = name;
            Hash = 0x7dfcee6a;
        }

        private int Query(uint alias)
        {
            if (Forbidden.Contains(alias))
            {
                return 0;
            }
            if (Limited.Contains(alias))
            {
                return 1;
            }
            if (SemiLimited.Contains(alias))
            {
                return 2;
            }
            return 3;
        }

		public int Query(CardData data)
		{
			uint alias = data.Alias > 0 ? data.Alias : data.Code;
			return Query(alias);
		}

        public void Add(uint cardId, int qualification)
        {
            switch (qualification)
            {
                case 0:
                    Forbidden.Add(cardId);
                    break;
                case 1:
                    Limited.Add(cardId);
                    break;
                case 2:
                    SemiLimited.Add(cardId);
                    break;
                default:
                    return;
            }
            uint code = cardId;
            Hash = Hash ^ ((code << 18) | (code >> 14)) ^ ((code << (27 + qualification)) | (code >> (5 - qualification)));
        }
    }
}
