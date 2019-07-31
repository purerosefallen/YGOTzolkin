using System.Collections;
using TMPro;
using UnityEngine;
using YGOTzolkin.Utility;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.Elements
{
    class Chain : IPoolSupporter
    {
        public int ChainSequence { get; set; }
        public ClientCard ChainCard { get; set; }
        //public int Description { get; set; }
        public Coordinate Coordinate { get; set; }
        public uint ChainCode { get; set; }
        //public List<ClientCard> Targets { get; set; }

        private bool showing;
        private bool solved;
        private GameObject chainObject;
        private TextMeshPro sequenceText;
        private Transform ring;

        public Chain()
        {
            solved = false;
            showing = false;
            //Targets = new List<ClientCard>();
            chainObject = MonoBehaviour.Instantiate(Resources.Load("Prefabs/Chain") as GameObject);
            chainObject.transform.localScale = Vector3.zero;
            sequenceText = chainObject.transform.GetChild(1).GetComponent<TextMeshPro>();
            ring = chainObject.transform.GetChild(0);
        }

        public void Reset()
        {
        }

        public void Disable()
        {
            solved = true;
            showing = false;
            chainObject.transform.localScale = Vector3.zero;
        }

        public void Activate(Vector3 tarPos)
        {
            solved = false;
            sequenceText.text = ChainSequence.ToString();
            chainObject.transform.position = tarPos;
            if (Coordinate.Location == (int)CardLocation.Hand)
            {
                chainObject.transform.rotation = Quaternion.Euler(new Vector3(-30, 0, 0));
            }
            else
            {
                chainObject.transform.rotation = Quaternion.Euler(Vector3.zero);
            }
            Animator.Instance.Play(new DuelAnimation(Spin(), 0));
        }

        public void ChainDisabled()
        {
            //diabled
        }

        public void Solving()
        {
            Animator.Instance.Play(new DuelAnimation(Twinkle()));
        }

        private IEnumerator Spin()
        {
            while (!solved)
            {
                if (MainGame.Instance.Field.CurrentChains.Count > 1)
                {
                    showing = true;
                    chainObject.transform.localScale = Vector3.one;
                    break;
                }
                yield return null;
            }

            while (!solved)
            {
                if (showing)
                {
                    ring.Rotate(new Vector3(0, 4, 0), Space.Self);
                    yield return null;
                }
            }
        }

        private IEnumerator Twinkle()
        {
            if (showing)
            {
                for (int i = 0; i < 6; ++i)
                {
                    chainObject.transform.localScale = Vector3.zero;
                    yield return Animator.WaitTime(0.05f);
                    chainObject.transform.localScale = Vector3.one;
                    yield return Animator.WaitTime(0.05f);
                }
            }
            solved = true;
            showing = false;
            chainObject.transform.localScale = Vector3.zero;
        }
    }
}
