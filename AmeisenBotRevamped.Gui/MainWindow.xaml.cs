using AmeisenBotRevamped.OffsetLists;
using AmeisenBotRevamped.Utils;
using AmeisenBotRevamped;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
using AmeisenBotRevamped.Gui.Views;
using AmeisenBotRevamped.DataAdapters;
using Magic;
using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.Clients;
using AmeisenBotRevamped.EventAdapters;
using System.Timers;
using AmeisenBotRevamped.Autologin;
using AmeisenBotRevamped.Autologin.Structs;
using Newtonsoft.Json;
using System.IO;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;

namespace AmeisenBotRevamped.Gui
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<AmeisenBot> AmeisenBots { get; set; }
        private List<BotView> BotViews { get; set; }

        private Timer ViewUpdateTimer { get; set; }
        private Timer BotFleetTimer { get; set; }

        private Settings Settings { get; set; }

        private Dictionary<string, bool> WowStartupMap { get; set; }

        private IOffsetList OffsetList { get; set; }

        private string SettingsPath => AppDomain.CurrentDomain.BaseDirectory + "config.json";

        public List<WowAccount> BotFleetAccounts { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            OffsetList = new Wotlk335a12340OffsetList();
            WowStartupMap = new Dictionary<string, bool>();

            ViewUpdateTimer = new Timer(1000);
            ViewUpdateTimer.Elapsed += CUpdateViews;

            BotFleetTimer = new Timer(1000);
            BotFleetTimer.Elapsed += CBotFleetTimer;

            LoadSettings();
        }

        #region UIEvents
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AmeisenBots = new List<AmeisenBot>();
            BotViews = new List<BotView>();

            BotFleetAccounts = ReadBotFleetAccounts();

            ScanForWows();
            RefreshActiveWows();

            ViewUpdateTimer.Start();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewUpdateTimer.Stop();
            BotFleetTimer.Stop();

            foreach (AmeisenBot ameisenBot in AmeisenBots)
            {
                ameisenBot.Detach();
            }
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void ButtonClose_Click(object sender, RoutedEventArgs e) => Close();

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            CheckForBotFleet();
        }

        private void ButtonToggleFleet_Click(object sender, RoutedEventArgs e)
        {
            if (BotFleetTimer.Enabled)
            {
                buttonToggleFleet.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF9696"));
                buttonToggleFleet.Content = "Fleet OFF";
                BotFleetTimer.Stop();
            }
            else
            {
                buttonToggleFleet.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB4FF96"));
                buttonToggleFleet.Content = "Fleet ON";
                BotFleetTimer.Start();
            }
        }
        #endregion

        #region TimerCallbacks
        private void CBotFleetTimer(object sender, ElapsedEventArgs e)
        {
            CheckForBotFleet();
        }

        private void CUpdateViews(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (BotView botView in BotViews)
                {
                    Dispatcher.Invoke(() => botView.UpdateView());
                }
            }
            catch { }
        }
        #endregion


        private void LoadSettings()
        {
            if (File.Exists(SettingsPath))
            {
                Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsPath));
            }
            else
            {
                Settings = new Settings();
                File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(Settings));
            }
        }


        private void ScanForWows()
        {
            List<AmeisenBot> AmeisenBotsNew = new List<AmeisenBot>();
            foreach (WowProcess wowProcess in BotUtils.GetRunningWows(OffsetList))
            {
                AmeisenBotsNew.Add(SetupAmeisenBot(wowProcess.Process));
            }
            AmeisenBots = AmeisenBotsNew;
        }

        private void RefreshActiveWows()
        {
            mainWrappanel.Children.Clear();
            BotViews = new List<BotView>();

            foreach (AmeisenBot ameisenBot in AmeisenBots)
            {
                if (ameisenBot.WowDataAdapter.GameState == WowGameState.Crashed)
                {
                    ameisenBot.Detach();
                    GC.Collect();

                    WowStartupMap[ameisenBot.CharacterName] = false;
                    AmeisenBots.Remove(ameisenBot);
                    break;
                }

                AddBotToView(ameisenBot);

                if (!ameisenBot.Attached && BotFleetAccounts.Where(acc => acc.CharacterName == ameisenBot.CharacterName).ToList().Count > 0)
                    AttachBot(ameisenBot);
            }
        }


        private AmeisenBot SetupAmeisenBot(Process wowProcess)
        {
            BlackMagic blackMagic = new BlackMagic(wowProcess.Id);

            IAutologinProvider autologinProvider = new SimpleAutologinProvider();
            IWowDataAdapter wowDataAdapter = new MemoryWowDataAdapter(blackMagic, OffsetList);

            return new AmeisenBot(blackMagic, wowDataAdapter, autologinProvider, wowProcess);
        }

        private void AttachBot(AmeisenBot ameisenBot)
        {
            if (ameisenBot.Attached)
            {
                ameisenBot.Detach();
            }
            else
            {
                IWowActionExecutor wowActionExecutor = new MemoryWowActionExecutor(ameisenBot.BlackMagic, OffsetList);
                IPathfindingClient pathfindingClient = new AmeisenNavPathfindingClient(Settings.AmeisenNavmeshServerIp, Settings.AmeisenNavmeshServerPort);
                IWowEventAdapter wowEventAdapter = new LuaHookWowEventAdapter(wowActionExecutor);
                ameisenBot.Attach(wowActionExecutor, pathfindingClient, wowEventAdapter);
            }
        }

        private void AddBotToView(AmeisenBot ameisenBot)
        {
            BotView botview = new BotView(ameisenBot, Settings, AttachBot);

            if (!BotViews.Contains(botview))
            {
                BotViews.Add(botview);
                mainWrappanel.Children.Add(botview);
            }
        }


        private void CheckForBotFleet()
        {
            if (BotFleetAccounts.Count > 0)
            {
                foreach (WowAccount wowAccount in BotFleetAccounts)
                {
                    if (!WowStartupMap.ContainsKey(wowAccount.CharacterName))
                        WowStartupMap.Add(wowAccount.CharacterName, false);

                    if (!CharacterIsLoggedIn(wowAccount.CharacterName)
                        && !LoginForCharacterIsInProgress(wowAccount.CharacterName)
                        && !WowStartupMap[wowAccount.CharacterName])
                    {
                        List<AmeisenBot> avaiableBots = AmeisenBots.Where(
                            bot => bot.CharacterName == ""
                            && !bot.AutologinProvider.LoginInProgress
                            && bot.WowDataAdapter.GameState != WowGameState.Crashed
                        ).ToList();

                        AmeisenBot ameisenBot = null;

                        if (avaiableBots.Count < 1)
                        {
                            if (Settings.WowExecutableFilePath != "" && File.Exists(Settings.WowExecutableFilePath))
                            {
                                WowStartupMap[wowAccount.CharacterName] = true;

                                Process newWowProcess = Process.Start(Settings.WowExecutableFilePath);
                                newWowProcess.WaitForInputIdle();

                                ameisenBot = SetupAmeisenBot(newWowProcess);
                                AmeisenBots.Add(ameisenBot);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            ameisenBot = avaiableBots.First();
                        }

                        if (ameisenBot == null) continue;

                        ameisenBot.AutologinProvider.DoLogin(ameisenBot.Process, wowAccount, OffsetList);
                        ameisenBot.ClearCaches();

                        try
                        {
                            Dispatcher.Invoke(() => AddBotToView(ameisenBot));
                        }
                        catch { }

                        WowStartupMap[ameisenBot.CharacterName] = false;
                    }
                }
            }

            try
            {
                Dispatcher.Invoke(() => RefreshActiveWows());
            }
            catch { }
        }

        private List<WowAccount> ReadBotFleetAccounts()
        {
            if (Settings.BotListFilePath != "" && File.Exists(Settings.BotListFilePath))
                return JsonConvert.DeserializeObject<List<WowAccount>>(File.ReadAllText(Settings.BotListFilePath));
            else
                return new List<WowAccount>();
        }


        private bool LoginForCharacterIsInProgress(string characterName)
        {
            foreach (AmeisenBot bot in AmeisenBots) if (bot.AutologinProvider.LoginInProgressCharactername == characterName) return true;
            return false;
        }

        private bool CharacterIsLoggedIn(string characterName)
        {
            foreach (AmeisenBot bot in AmeisenBots) if (bot.CharacterName == characterName) return true;
            return false;
        }
    }
}
