using System.Collections.Generic;
using UnityEngine;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin
{
    class FieldInfo
    {
        public static Dictionary<int, Dictionary<CardPosition, Quaternion>> FieldRotations { get; private set; }
        public static Dictionary<CardPosition, Quaternion> HandRotations { get; private set; }
        public static Dictionary<int, Vector3> SZonePositions { get; private set; }
        public static Dictionary<int, Vector3> MZonePositions { get; private set; }
        public static Dictionary<CardLocation, Vector3> BasePositions { get; private set; }
        public static Vector3[] ConfirmPosition { get; private set; }
        public static Vector3[] ShuffleSetPosition { get; private set; }
        static FieldInfo()
        {
            var dic1 = new Dictionary<CardPosition, Quaternion>()
            {
                { CardPosition.FaceUp,      Quaternion.Euler( new Vector3(90,0,0))},
                { CardPosition.FaceUpAttack,Quaternion.Euler( new Vector3(90,0,0)) },
                { CardPosition.Attack,      Quaternion.Euler( new Vector3(90,0,0)) },

                { CardPosition.FaceDown,        Quaternion.Euler( new Vector3(270,90,90)) },
                { CardPosition.FaceDownAttack,  Quaternion.Euler( new Vector3(270,90,90)) },

                { CardPosition.FaceUpDefence,   Quaternion.Euler( new Vector3(90,0,90)) },
                { CardPosition.Defence,         Quaternion.Euler( new Vector3(90,0,90)) },

                { CardPosition.FaceDownDefence, Quaternion.Euler( new Vector3(270,0,90)) },
            };
            var dic2 = new Dictionary<CardPosition, Quaternion>()
            {
                { CardPosition.FaceUp,      Quaternion.Euler( new Vector3(90,0,180)) },
                { CardPosition.FaceUpAttack,Quaternion.Euler( new Vector3(90,0,180)) },
                { CardPosition.Attack,      Quaternion.Euler( new Vector3(90,0,180)) },

                { CardPosition.FaceDown,        Quaternion.Euler( new Vector3(270,90,270)) },
                { CardPosition.FaceDownAttack,  Quaternion.Euler( new Vector3(270,90,270)) },

                { CardPosition.FaceUpDefence,   Quaternion.Euler( new Vector3(90,0,270)) },
                { CardPosition.Defence,         Quaternion.Euler( new Vector3(90,0,270)) },

                { CardPosition.FaceDownDefence, Quaternion.Euler( new Vector3(-90,0,270)) },
            };
            FieldRotations = new Dictionary<int, Dictionary<CardPosition, Quaternion>>
            {
                { 0, dic1 },
                { 1, dic2 }
            };
            HandRotations = new Dictionary<CardPosition, Quaternion>
            {
                { CardPosition.FaceUp,      Quaternion.Euler( new Vector3(60, 0, 0))},
                { CardPosition.FaceDown,    Quaternion.Euler( new Vector3(-60, 180, 0)) },
            };
            Vector3 stride = new Vector3(10.6f, 0, 0);
            SZonePositions = new Dictionary<int, Vector3>();
            MZonePositions = new Dictionary<int, Vector3>
            {
                {5,new Vector3 (-10.6f, 0, 0) },
                {6,new Vector3 (10.6f, 0, 0) },
            };
            for (int i = 0; i < 5; ++i)
            {
                SZonePositions.Add(i, new Vector3(-21.2f, 0, -21.2f) + i * stride);
                MZonePositions.Add(i, new Vector3(-21.2f, 0, -10.6f) + i * stride);
            }
            SZonePositions.Add(5, new Vector3(-30.45f, 0, -15.9f));
            BasePositions = new Dictionary<CardLocation, Vector3>
            {
                { CardLocation.Deck,new Vector3(30.45f, 0, -26.5f) },
                { CardLocation.Extra,new Vector3(-30.45f, 0, -26.5f) },
                { CardLocation.Grave,new Vector3(30.45f, 0, -15.9f)  },
                { CardLocation.Removed,new Vector3(30.45f, 0, -5.3f) },
            };
            ConfirmPosition = new Vector3[2];
            ConfirmPosition[0] = Vector3.Lerp(new Vector3(0, 0, -15), MainGame.Instance.MainCamera.transform.position, 0.3f);
            ConfirmPosition[1] = Vector3.Lerp(new Vector3(0, 0, 15), MainGame.Instance.MainCamera.transform.position, 0.3f);
            ShuffleSetPosition = new Vector3[2]
            {
                new Vector3(MZonePositions[2].x,2,MZonePositions[2].z),
                new Vector3(MZonePositions[2].x*-1,2,MZonePositions[2].z*-1),
            };
        }
    }
}
