using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YGOTzolkin.Service;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.UI
{
    class ImageSelector : WindowBase
    {
        private RectTransform mainRect;

        private List<Toggle> toggles;

        public ImageSelector()
        {
            MainCanvas = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/ImageSelector") as GameObject).GetComponent<Canvas>();
            toggles = new List<Toggle>(4);
            TryGetControl(out mainRect, "Panel");
        }

        internal void SelectHand()
        {
            for (int i = 1; i < 4; ++i)
            {
                var response = new byte[]
                {
                    0x02,
                    0x00,
                    (byte)CToSMessage.HandResult,
                    (byte)i,
                };
                var texture = TextureService.ReadTexture(string.Format("textures/f{0}.jpg", i));
                var toggle = AddToggle();
                if(texture!=null)
                {
                    var img = toggle.transform.GetChild(0).GetComponent<Image>();
                    img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        NetworkService.Instance.Send(response);
                        Hide();
                    }
                });
            }
            Show();
        }

        internal void SelectPosition(uint code, int positions)
        {
            int filter = 0x1;
            while (filter != 0x10)
            {
                if ((positions & filter) > 0)
                {
                    int response = filter;
                    var tgl = AddToggle();
                    tgl.onValueChanged.AddListener((isOn) =>
                    {
                        if (isOn)
                        {
                            MainGame.Instance.SendCToSResponse(response);
                            Hide();
                        }
                    });
                    var img = tgl.transform.GetChild(0).GetComponent<Image>();
                    Texture2D texture;
                    switch (filter)
                    {
                        case 0x1:
                            texture = TextureService.GetCardTexture(code);
                            break;
                        case 0x2:
                            texture = TextureService.Cover;
                            break;
                        case 0x4:
                            texture = TextureService.GetCardTexture(code);
                            tgl.transform.Rotate(new Vector3(0, 0, 90));
                            break;
                        case 0x8:
                            texture = TextureService.Cover;
                            tgl.transform.Rotate(new Vector3(0, 0, 90));
                            break;
                        default:
                            texture = new Texture2D(1, 1);
                            break;
                    }
                    img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                filter <<= 1;
            }
            Show();
        }

        private Toggle AddToggle()
        {
            var toggle = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/OptionImage") as GameObject).GetComponent<Toggle>();
            toggles.Add(toggle);
            toggle.transform.SetParent(mainRect, false);
            toggle.transform.localScale = Vector3.one;
            return toggle;
        }

        internal override void Hide()
        {
            base.Hide();
            foreach (var toggle in toggles)
            {
                toggle.transform.SetParent(null);
                UnityEngine.Object.Destroy(toggle.gameObject);
            }
            toggles.Clear();
        }
    }
}
