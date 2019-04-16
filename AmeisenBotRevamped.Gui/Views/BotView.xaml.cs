using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.Clients;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using AmeisenBotRevamped.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        public delegate void AttachBotFunction(AmeisenBot ameisenBot);

        private AmeisenBot AmeisenBot { get; set; }
        private AttachBotFunction AttachBotFunc { get; set; }
        private Settings Settings { get; set; }

        public BotView(AmeisenBot ameisenBot, Settings settings, AttachBotFunction attachBotFunction)
        {
            AmeisenBot = ameisenBot;
            Settings = settings;
            AttachBotFunc = attachBotFunction;

            InitializeComponent();

            UpdateView();
        }

        public void UpdateView()
        {
            if (AmeisenBot.WowDataAdapter.GameState == WowGameState.Crashed) return;

            if (AmeisenBot.Attached)
            {
                buttonAttach.Content = "Detach";
                buttonAttach.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB4FF96"));
            }
            else
            {
                buttonAttach.Content = "Attach";
                buttonAttach.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF9696"));
            }

            ulong playerGuid = AmeisenBot.WowDataAdapter.PlayerGuid;
            WowPlayer player = (WowPlayer)AmeisenBot.ObjectManager.GetWowObjectByGuid(playerGuid);

            ulong targetGuid = AmeisenBot.WowDataAdapter.TargetGuid;
            WowUnit target = (WowUnit)AmeisenBot.ObjectManager.GetWowObjectByGuid(targetGuid);

            if (AmeisenBot.CharacterName.Length == 0)
                labelBotname.Content = "Not logged in";
            else
                labelBotname.Content = AmeisenBot.CharacterName;

            labelBotrealm.Content = $"<{AmeisenBot.RealmName}>";
            labelBotmapinfo.Content = $"<{AmeisenBot.WowDataAdapter.ContinentName}> <{AmeisenBot.WowDataAdapter.MapId}, {AmeisenBot.WowDataAdapter.ZoneId}>";
            labelBotgamestate.Content = $"<{AmeisenBot.WowDataAdapter.GameState.ToString()}>";
            labelBotaccount.Content = $"<{AmeisenBot.WowDataAdapter.AccountName}>";

            if (AmeisenBot.StateMachine != null)
                labelBotstate.Content = $"<{AmeisenBot.StateMachine.CurrentState.ToString()}>";

            if (player != null)
            {
                labelBothealth.Content = $"Health: {BotUtils.BigValueToString(player.Health)}/{BotUtils.BigValueToString(player.MaxHealth)}";

                if (player.MaxHealth > 0)
                    progressbarBothealth.Value = (player.Health / player.MaxHealth) * 100;

                labelBotenergy.Content = $"Energy: {BotUtils.BigValueToString(player.Energy)}/{BotUtils.BigValueToString(player.MaxEnergy)}";

                if (player.MaxEnergy > 0)
                    progressbarBotenergy.Value = (player.Energy / player.MaxEnergy) * 100;

                labelBotexp.Content = $"Exp: {BotUtils.BigValueToString(player.Exp)}/{BotUtils.BigValueToString(player.MaxExp)}";

                if (player.MaxExp > 0)
                    progressbarBotexp.Value = (player.Exp / player.MaxExp) * 100;

                labelBotlevel.Content = $"lvl. {player.Level}";
                labelBotraceclass.Content = $"<{player.Race.ToString()}, {player.Class.ToString()}>";

                if (target != null)
                    labelBotdebug.Content = $"0x{target.BaseAddress.ToString("X")}";

                BitmapImage botBitmap = SearchForBotPicture();
                if (botBitmap != null)
                {
                    botImage.Source = botBitmap;
                }

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

        private BitmapImage SearchForBotPicture()
        {
            if (Directory.Exists(Settings.BotPictureFolder))
            {
                string pngPath = $"{Settings.BotPictureFolder}/{AmeisenBot.CharacterName.ToLower(CultureInfo.CurrentCulture)}.png";
                string jpgPath = $"{Settings.BotPictureFolder}/{AmeisenBot.CharacterName.ToLower(CultureInfo.CurrentCulture)}.jpg";

                if (File.Exists(pngPath))
                {
                    return new BitmapImage(new Uri(pngPath));
                }
                else if (File.Exists(jpgPath))
                {
                    return new BitmapImage(new Uri(jpgPath));
                }
            }

            return null;
        }

        private void ButtonAttach_Click(object sender, RoutedEventArgs e)
        {
            AttachBotFunc?.Invoke(AmeisenBot);
            UpdateView();
        }
    }
}
