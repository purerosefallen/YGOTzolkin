using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using YGOTzolkin.Utility;

namespace YGOTzolkin.UI
{
    class ToolStrip : WindowBase
    {
        public ToolStrip()
        {
            MainCanvas = Tools.LoadResource<Canvas>("Prefabs/ToolStrip");
            TryGetControl(out Button config, "btnconfig");
            config.onClick.AddListener(OnConfig);
        }

        private void OnConfig()
        {
            MainGame.Instance.ConfigWindow.Show();
        }
    }
}
