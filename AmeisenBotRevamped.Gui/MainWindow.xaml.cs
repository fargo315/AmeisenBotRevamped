using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.AI.CombatEngine.MovementProvider;
using AmeisenBotRevamped.Autologin;
using AmeisenBotRevamped.Autologin.Structs;
using AmeisenBotRevamped.Clients;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.EventAdapters;
using AmeisenBotRevamped.Gui.Views;
using AmeisenBotRevamped.Logging;
using AmeisenBotRevamped.Logging.Enums;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using AmeisenBotRevamped.OffsetLists;
using AmeisenBotRevamped.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TrashMemCore;

namespace AmeisenBotRevamped.Gui
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<AmeisenBot> AmeisenBots { get; set; }
        private List<BotView> BotViews { get; set; }

        private System.Timers.Timer ViewUpdateTimer { get; }
        private System.Timers.Timer BotFleetTimer { get; }

        private Settings Settings { get; set; }

        private Dictionary<string, bool> WowStartupMap { get; }

        private IOffsetList OffsetList { get; }

        private string SettingsPath => AppDomain.CurrentDomain.BaseDirectory + "config.json";

        public List<WowAccount> BotFleetAccounts { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            AmeisenBotLogger.Instance.ActiveLogLevel = LogLevel.Verbose;
            AmeisenBotLogger.Instance.Start();
            AmeisenBotLogger.Instance.Log("AmeisenBotGui loading...");

            OffsetList = new Wotlk335a12340OffsetList();
            WowStartupMap = new Dictionary<string, bool>();

            ViewUpdateTimer = new System.Timers.Timer(1000);
            ViewUpdateTimer.Elapsed += CUpdateViews;

            BotFleetTimer = new System.Timers.Timer(2000);
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
            AmeisenBotLogger.Instance.Log("AmeisenBotGui closing...");

            ViewUpdateTimer.Stop();
            BotFleetTimer.Stop();

            foreach (AmeisenBot ameisenBot in AmeisenBots)
            {
                ameisenBot.Detach();
            }

            AmeisenBotLogger.Instance.Stop();
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void ButtonClose_Click(object sender, RoutedEventArgs e) => Close();

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            ScanForWows();
            RefreshActiveWows();
        }

        private void ButtonToggleFleet_Click(object sender, RoutedEventArgs e)
        {
            if (BotFleetTimer.Enabled)
            {
                AmeisenBotLogger.Instance.Log($"FleetMode disabled");
                buttonToggleFleet.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF171717"));
                buttonToggleFleet.Content = "Fleet-Mode OFF";
                BotFleetTimer.Stop();
            }
            else
            {
                AmeisenBotLogger.Instance.Log($"FleetMode enabled");
                buttonToggleFleet.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078FF"));
                buttonToggleFleet.Content = "Fleet-Mode ON";
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
                AmeisenBotLogger.Instance.Log($"Loading settings from \"{SettingsPath}\"");
                Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsPath));
            }
            else
            {
                AmeisenBotLogger.Instance.Log($"Writing default settings to \"{SettingsPath}\"");
                Settings = new Settings();
                File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(Settings));
            }
        }

        private void SaveSetings()
        {
            File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(Settings));
        }

        private void ScanForWows()
        {
            AmeisenBotLogger.Instance.Log("Scanning for active WoW processes...");

            List<AmeisenBot> AmeisenBotsNew = new List<AmeisenBot>();
            foreach (WowProcess wowProcess in BotUtils.GetRunningWows(OffsetList))
            {
                AmeisenBotLogger.Instance.Log($"Found WoW process with PID: {wowProcess.Process.Id.ToString("X")} [CharacterName: \"{wowProcess.CharacterName}\", RealmName: \"{wowProcess.RealmName}\", IsHooked: \"{wowProcess.IsHooked}\"]");
                AmeisenBotsNew.Add(SetupAmeisenBot(wowProcess.Process));
            }
            AmeisenBots = AmeisenBotsNew;
        }

        private void RefreshActiveWows(bool autoAttach = false)
        {
            mainWrappanel.Children.Clear();
            BotViews = new List<BotView>();

            try
            {
                foreach (AmeisenBot ameisenBot in AmeisenBots)
                {
                    if (ameisenBot.WowDataAdapter.GameState == WowGameState.Crashed || ameisenBot.Process.HasExited)
                    {
                        AmeisenBotLogger.Instance.Log($"[{ameisenBot.Process.Id.ToString("X")}]\tRemoving crashed AmeisenBot...");

                        ameisenBot.Detach();
                        GC.Collect();

                        WowStartupMap[ameisenBot.CharacterName] = false;
                        AmeisenBots.Remove(ameisenBot);
                        break;
                    }

                    AddBotToView(ameisenBot);

                    if (autoAttach && !ameisenBot.Attached && BotFleetAccounts.Where(acc => acc.CharacterName == ameisenBot.CharacterName).ToList().Count > 0)
                    {
                        // Causing random crashes, don't know why
                        //AttachBot(ameisenBot);
                    }
                }
            }
            catch { }
        }

        private AmeisenBot SetupAmeisenBot(Process wowProcess)
        {
            AmeisenBotLogger.Instance.Log($"[{wowProcess.Id.ToString("X")}]\tSetting up the AmeisenBot...");

            TrashMem trashMem = new TrashMem(wowProcess);

            IAutologinProvider autologinProvider = new SimpleAutologinProvider();
            IWowDataAdapter wowDataAdapter = new MemoryWowDataAdapter(trashMem, OffsetList);

            return new AmeisenBot(trashMem, wowDataAdapter, autologinProvider, wowProcess);
        }

        private void AttachBot(AmeisenBot ameisenBot)
        {
            if (ameisenBot.Attached)
            {
                AmeisenBotLogger.Instance.Log($"[{ameisenBot.Process.Id.ToString("X")}]\tDetaching AmeisenBot..");

                ameisenBot.Detach();
            }
            else
            {
                AmeisenBotLogger.Instance.Log($"[{ameisenBot.Process.Id.ToString("X")}]\tAttaching AmeisenBot...");

                IWowActionExecutor wowActionExecutor = new MemoryWowActionExecutor(ameisenBot.TrashMem, OffsetList);
                IPathfindingClient pathfindingClient = new AmeisenNavPathfindingClient(Settings.AmeisenNavmeshServerIp, Settings.AmeisenNavmeshServerPort, ameisenBot.Process.Id);
                IWowEventAdapter wowEventAdapter = new LuaHookWowEventAdapter(wowActionExecutor);
                ameisenBot.Attach(wowActionExecutor, pathfindingClient, wowEventAdapter, new BasicMeleeMovementProvider(), null);
            }
        }

        private void AddBotToView(AmeisenBot ameisenBot)
        {
            AmeisenBotLogger.Instance.Log($"[{ameisenBot.Process.Id.ToString("X")}]\tAdding AmeisenBot to view...");
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
                    {
                        WowStartupMap.Add(wowAccount.CharacterName, false);
                    }

                    if (!CharacterIsLoggedIn(wowAccount.CharacterName)
                        && !LoginForCharacterIsInProgress(wowAccount.CharacterName)
                        && !WowStartupMap[wowAccount.CharacterName])
                    {
                        List<AmeisenBot> avaiableBots = AmeisenBots.Where(
                            bot => bot.CharacterName.Length != 0
                            && !bot.AutologinProvider.LoginInProgress
                            && bot.WowDataAdapter.GameState != WowGameState.Crashed
                        ).ToList();

                        AmeisenBot ameisenBot = null;

                        if (avaiableBots.Count < 1)
                        {
                            if (Settings.WowExePath.Length != 0 && File.Exists(Settings.WowExePath))
                            {
                                WowStartupMap[wowAccount.CharacterName] = true;

                                Process newWowProcess = Process.Start(Settings.WowExePath);
                                AmeisenBotLogger.Instance.Log($"[{newWowProcess.Id.ToString("X")}]\tStarting new WoW process..");
                                newWowProcess.WaitForInputIdle();

                                ameisenBot = SetupAmeisenBot(newWowProcess);

                                if (Settings.WowPositions.ContainsKey(wowAccount.CharacterName))
                                {
                                    ameisenBot.SetWindowPosition(Settings.WowPositions[wowAccount.CharacterName]);
                                }

                                Thread.Sleep(200);

                                AmeisenBots.Add(ameisenBot);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            ameisenBot = avaiableBots[0];
                        }

                        if (ameisenBot == null)
                        {
                            continue;
                        }

                        AmeisenBotLogger.Instance.Log($"[{ameisenBot.Process.Id.ToString("X")}]\tLoggin into account \"{wowAccount.Username}\"");
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
                Dispatcher.Invoke(() => RefreshActiveWows(true));
            }
            catch { }
        }

        private List<WowAccount> ReadBotFleetAccounts()
        {
            AmeisenBotLogger.Instance.Log($"Reading FleetConfig from \"{Settings.BotFleetConfig}\"");

            if (Settings.BotFleetConfig.Length != 0 && File.Exists(Settings.BotFleetConfig))
            {
                return JsonConvert.DeserializeObject<List<WowAccount>>(File.ReadAllText(Settings.BotFleetConfig));
            }
            else
            {
                return new List<WowAccount>();
            }
        }

        private bool LoginForCharacterIsInProgress(string characterName)
        {
            foreach (AmeisenBot bot in AmeisenBots)
            {
                if (bot.AutologinProvider.LoginInProgressCharactername == characterName)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CharacterIsLoggedIn(string characterName)
        {
            foreach (AmeisenBot bot in AmeisenBots)
            {
                if (bot.CharacterName == characterName)
                {
                    return true;
                }
            }

            return false;
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(Settings, AmeisenBots);
            settingsWindow.ShowDialog();

            Settings = settingsWindow.Settings;
            SaveSetings();
        }
    }
}
