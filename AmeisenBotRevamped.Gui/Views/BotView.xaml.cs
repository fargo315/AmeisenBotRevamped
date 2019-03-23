using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AmeisenBotRevamped.Gui.Views
{
    /// <summary>
    /// Interaktionslogik für BotView.xaml
    /// </summary>
    public partial class BotView : UserControl
    {
        private AmeisenBot AmeisenBot { get; set; }

        public BotView(AmeisenBot ameisenBot)
        {
            AmeisenBot = ameisenBot;
            InitializeComponent();
            UpdateView();
        }

        public void UpdateView()
        {
            if (AmeisenBot.WowDataAdapter.GameState == WowGameState.Crashed) return;

            ulong playerGuid = AmeisenBot.WowDataAdapter.PlayerGuid;
            WowPlayer player = (WowPlayer)AmeisenBot.ObjectManager.GetWowObjectByGuid(playerGuid);

            if (AmeisenBot.CharacterName == "")
                labelBotname.Content = "Not logged in";
            else
                labelBotname.Content = AmeisenBot.CharacterName;

            labelBotrealm.Content = $"<{AmeisenBot.RealmName}>";
            labelBotgamestate.Content = $"<{AmeisenBot.WowDataAdapter.GameState.ToString()}>";

            if (AmeisenBot.StateMachine != null)
                labelBotstate.Content = $"<{AmeisenBot.StateMachine.CurrentState.ToString()}>";

            if (player != null)
            {
                labelBothealth.Content = $"Health: {player.Health}/{player.MaxHealth}";

                if (player.MaxHealth > 0)
                    progressbarBothealth.Value = (player.Health / player.MaxHealth) * 100;

                labelBotenergy.Content = $"Energy: {player.Energy}/{player.MaxEnergy}";

                if (player.MaxEnergy > 0)
                    progressbarBotenergy.Value = (player.Energy / player.MaxEnergy) * 100;

                labelBotlevel.Content = $"lvl. {player.Level}";
                labelBotraceclass.Content = $"<{player.Race.ToString()}, {player.Class.ToString()}>";
                labelBotdebug.Content = $"0x{player.BaseAddress.ToString("X")} => 0x{player.DescriptorAddress.ToString("X")}";

                switch (player.Class)
                {
                    case WowClass.DeathKnight:
                        progressbarBothealth.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C41F3B"));
                        progressbarBotenergy.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8FFFEB"));
                        break;

                    case WowClass.Druid:
                        progressbarBothealth.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7D0A"));
                        progressbarBotenergy.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8FC2FF"));
                        break;

                    case WowClass.Hunter:
                        progressbarBothealth.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ABD473"));
                        progressbarBotenergy.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8FC2FF"));
                        break;

                    case WowClass.Mage:
                        progressbarBothealth.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#69CCF0"));
                        progressbarBotenergy.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8FC2FF"));
                        break;

                    case WowClass.Paladin:
                        progressbarBothealth.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F58CBA"));
                        progressbarBotenergy.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8FC2FF"));
                        break;

                    case WowClass.Priest:
                        progressbarBothealth.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                        progressbarBotenergy.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8FC2FF"));
                        break;

                    case WowClass.Rogue:
                        progressbarBothealth.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF569"));
                        progressbarBotenergy.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFF88F"));
                        break;

                    case WowClass.Shaman:
                        progressbarBothealth.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0070DE"));
                        progressbarBotenergy.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8FC2FF"));
                        break;

                    case WowClass.Warlock:
                        progressbarBothealth.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9482C9"));
                        progressbarBotenergy.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8FC2FF"));
                        break;

                    case WowClass.Warrior:
                        progressbarBothealth.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C79C6E"));
                        progressbarBotenergy.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE6E6E6"));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
