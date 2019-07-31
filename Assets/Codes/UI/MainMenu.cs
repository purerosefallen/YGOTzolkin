using System;
using UnityEngine;
using UnityEngine.UI;
using YGOTzolkin.Service;
using YGOTzolkin.Utility;

namespace YGOTzolkin.UI
{
    class MainMenu : WindowBase
    {
        public MainMenu()
        {
            MainCanvas = Tools.LoadResource<Canvas>("Prefabs/MainMenu");
            TryGetControl(out Button editDeck, "EditDeck");
            editDeck.onClick.AddListener(OnEditDeck);
            editDeck.SetButtonName(DataService.SysString(1204));
            TryGetControl(out Button duel, "Duel");
            duel.onClick.AddListener(OnDuel);
            duel.SetButtonName(DataService.SysString(1200));
            TryGetControl(out Button settings, "settings");
            settings.onClick.AddListener(OnSettings);
            settings.SetButtonName(DataService.TzlString(15));
        }

        internal override void Hide()
        {
            base.Hide();
            MainGame.Instance.ToolStrip.Show();
        }

        internal override void Show()
        {
            base.Show();
            MainGame.Instance.ToolStrip.Hide();
        }

        private void OnSettings()
        {
            MainGame.Instance.ConfigWindow.Show();
        }

        private void OnEditDeck()
        {
            Hide();
            MainGame.Instance.DeckBuilder.Show();
        }

        private void OnDuel()
        {
            Hide();
            MainGame.Instance.ServerWindow.Show();
        }
    }
}
