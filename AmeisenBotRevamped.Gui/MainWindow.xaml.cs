using AmeisenBotRevamped.Autologin.Structs;
using AmeisenBotRevamped.Gui.BotManager;
using AmeisenBotRevamped.Gui.BotManager.Objects;
using AmeisenBotRevamped.Gui.Views;
using AmeisenBotRevamped.Logging;
using AmeisenBotRevamped.Logging.Enums;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using AmeisenBotRevamped.OffsetLists;
using AmeisenBotRevamped.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AmeisenBotRevamped.Gui
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly object botViewsLock = new object();
        private static readonly string SettingsPath = AppDomain.CurrentDomain.BaseDirectory + "config.json";

        private List<AmeisenBot> UnmanagedAmeisenBots { get; set; }
        private List<BotView> BotViews { get; set; }

        private Timer ViewUpdateTimer { get; }

        private Settings Settings { get; set; }
        private IOffsetList OffsetList { get; }

        private AmeisenBotManager AmeisenBotManager { get; }

        public MainWindow()
        {
            InitializeComponent();

            UnmanagedAmeisenBots = new List<AmeisenBot>();

            AmeisenBotLogger.Instance.ActiveLogLevel = LogLevel.Verbose;
            AmeisenBotLogger.Instance.Start();
            AmeisenBotLogger.Instance.Log("AmeisenBotGui loading...");

            OffsetList = new Wotlk335a12340OffsetList();

            ViewUpdateTimer = new Timer(1000);
            ViewUpdateTimer.Elapsed += CUpdateViews;

            LoadSettings();

            AmeisenBotManager = new AmeisenBotManager(UnmanagedAmeisenBots, Settings, ReadBotFleetAccounts(), OffsetList);
        }

        #region UIEvents
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BotViews = new List<BotView>();
            ViewUpdateTimer.Start();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AmeisenBotLogger.Instance.Log("AmeisenBotGui closing...");

            foreach(BotView botView in BotViews)
            {
                botView.AmeisenBot.Detach();
            }

            ViewUpdateTimer.Stop();
            AmeisenBotManager.Dispose();

            AmeisenBotLogger.Instance.Stop();
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void ButtonClose_Click(object sender, RoutedEventArgs e) => Close();

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonToggleFleet_Click(object sender, RoutedEventArgs e)
        {
            if (AmeisenBotManager.Enabled)
            {
                AmeisenBotLogger.Instance.Log("FleetMode disabled");
                buttonToggleFleet.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4B4B4B"));
                buttonToggleFleet.Content = "Fleet-Mode OFF";
                AmeisenBotManager.Stop();
            }
            else
            {
                AmeisenBotLogger.Instance.Log("FleetMode enabled");
                buttonToggleFleet.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078FF"));
                buttonToggleFleet.Content = "Fleet-Mode ON";
                AmeisenBotManager.Start();
            }
        }
        #endregion

        #region TimerCallbacks
        private void CUpdateViews(object sender, ElapsedEventArgs e)
        {
            foreach (ManagedAmeisenBot bot in AmeisenBotManager.ManagedAmeisenBots)
            {
                Dispatcher.Invoke(() => BotViews.Add(new BotView(
                    bot.AmeisenBot,
                    Settings,
                    AmeisenBotManager.AttachAmeisenBotOnNewThread)));
            }

            foreach (WowProcess process in BotUtils.GetRunningWows(OffsetList))
            {
                if (!BotViews.Any(b => b.AmeisenBot.Process.Id == process.Process.Id)
                    && !AmeisenBotManager.ManagedAmeisenBots.Any(m => m.AmeisenBot.Process.Id == process.Process.Id))
                {
                    AmeisenBot newAmeisenBot = AmeisenBotManager.SetupAmeisenBot(process.Process);
                    Dispatcher.Invoke(() => BotViews.Add(new BotView(
                        newAmeisenBot,
                        Settings,
                        AmeisenBotManager.AttachAmeisenBotOnNewThread)));

                    UnmanagedAmeisenBots.Add(newAmeisenBot);
                }
            }

            List<BotView> newBotViews = new List<BotView>();
            foreach (BotView botView in BotViews)
            {
                if (!botView.AmeisenBot.Process.HasExited
                    && botView.AmeisenBot.WowDataAdapter.GameState != WowGameState.Crashed)
                {
                    newBotViews.Add(botView);

                    Dispatcher.Invoke(() => AddBotToView(botView));
                    Dispatcher.Invoke(() => botView.UpdateView());
                }
                else
                {
                    Dispatcher.Invoke(() => RemoveBotFromView(botView));
                }
            }
            BotViews = newBotViews;

            Dispatcher.Invoke(() => UpdateViews());
        }
        #endregion

        private void AddBotToView(BotView botView)
        {
            if (!mainWrappanel.Children.Contains(botView))
            {
                mainWrappanel.Children.Add(botView);
            }
        }

        private void RemoveBotFromView(BotView botView)
        {
            if (mainWrappanel.Children.Contains(botView))
            {
                mainWrappanel.Children.Remove(botView);
            }
        }

        private void UpdateViews()
        {
            labelActiveBotThreads.Content = AmeisenBotManager.AmeisenBotThreads.Count;
            labelActiveWatchdogs.Content = AmeisenBotManager.AmeisenBotWatchdogThreads.Count;
        }

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

        private List<WowAccount> ReadBotFleetAccounts()
        {
            AmeisenBotLogger.Instance.Log($"Reading FleetConfig from \"{Settings.BotFleetConfig}\"");

            if (Settings.BotFleetConfig.Length != 0
                && File.Exists(Settings.BotFleetConfig))
            {
                return JsonConvert.DeserializeObject<List<WowAccount>>(File.ReadAllText(Settings.BotFleetConfig));
            }
            else
            {
                return new List<WowAccount>();
            }
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(Settings, AmeisenBotManager.ManagedAmeisenBots);
            settingsWindow.ShowDialog();

            Settings = settingsWindow.Settings;
            SaveSetings();
        }
    }
}
