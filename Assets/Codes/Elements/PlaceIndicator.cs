using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using YGOTzolkin.Utility;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.Elements
{
    class PlaceIndicator : IPoolSupporter
    {
        private static readonly ObjectPool<PlaceIndicator> indicatorPool = new ObjectPool<PlaceIndicator>();
        private static readonly List<PlaceIndicator> placeIndicators = new List<PlaceIndicator>();

        public static void Create(int ctr, CardLocation loc, int seq)
        {
            var indicator = indicatorPool.New();
            indicator.coordBytes = new byte[]
            {
                (byte)GameInfo.Instance.LocalPlayer(ctr),
                (byte)loc,
                (byte)seq,
            };
            Vector3 pos;
            if (loc == CardLocation.MonsterZone)
            {
                pos = FieldInfo.MZonePositions[seq] * (-2 * ctr + 1);
            }
            else
            {
                pos = FieldInfo.SZonePositions[seq] * (-2 * ctr + 1);
            }
            indicator.indicatorObject.transform.position = pos;
            placeIndicators.Add(indicator);
        }

        public static void Clear()
        {
            indicatorPool.Store(placeIndicators);
            placeIndicators.Clear();
        }

        public static void CheckCount()
        {
            if (placeIndicators.Count == MainGame.Instance.Field.SelectMin)
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                foreach (var place in placeIndicators)
                {
                    writer.Write(place.coordBytes);
                    indicatorPool.Store(place);
                }
                MainGame.Instance.SendCToSResponse(stream.ToArray());
                placeIndicators.Clear();
            }
        }

        private readonly GameObject indicatorObject;

        private byte[] coordBytes;

        public bool Selected { get; private set; }

        public PlaceIndicator()
        {
            indicatorObject = Object.Instantiate(Resources.Load("Prefabs/PlaceIndicator") as GameObject);
            Tools.BindEvent(indicatorObject, EventTriggerType.PointerClick, OnClick);
        }

        public void Disable()
        {
            indicatorObject.transform.localScale = Vector3.zero;
        }

        public void Reset()
        {
            indicatorObject.transform.localScale = Vector3.one;
            Selected = false;
        }

        private void OnClick(BaseEventData data)
        {
            Selected = !Selected;
            var selected = placeIndicators.FindAll((p) => p.Selected);
            if (selected.Count == MainGame.Instance.Field.SelectMin)
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                selected.ForEach((p) => writer.Write(p.coordBytes));
                MainGame.Instance.SendCToSResponse(stream.ToArray());
                indicatorPool.Store(placeIndicators);
                placeIndicators.Clear();
            }
        }
    }
}
