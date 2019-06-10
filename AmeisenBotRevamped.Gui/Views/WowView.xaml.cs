using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using AmeisenBotRevamped.Utils;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AmeisenBotRevamped.Gui.Views
{
    /// <summary>
    /// Interaktionslogik für BotView.xaml
    /// </summary>
    public partial class WowView : UserControl, IAmeisenBotView
    {
        public delegate void AttachBotFunction(AmeisenBot ameisenBot);

        public WowProcess WowProcess { get; }

        Process IAmeisenBotView.Process => WowProcess.Process;

        public WowView(WowProcess process)
        {
            WowProcess = process;

            InitializeComponent();

            UpdateView();
        }

        public void UpdateView()
        {
            labelRealmName.Content = WowProcess.RealmName;
            labelProcessId.Content = $"{WowProcess.Process.Id.ToString("X")} ({WowProcess.Process.Id})";
            labelLoginInProgress.Content = $"LoginInProgress: {WowProcess.LoginInProgress}";

            if (WowProcess.CharacterName?.Length == 0)
                labelCharacterName.Content = WowProcess.CharacterName;
            else
                labelCharacterName.Content = "Not logged in";
        }
    }
}
