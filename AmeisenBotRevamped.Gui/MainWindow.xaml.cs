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
        private static readonly string SettingsPath = AppDomain.CurrentDomain.BaseDirectory + "config.json";

        private List<AmeisenBot> UnmanagedAmeisenBots { get; }

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

            OffsetList = new Wotlk335A12340OffsetList();

            ViewUpdateTimer = new Timer(1000);
            ViewUpdateTimer.Elapsed += CUpdateViews;

            LoadSettings();

            AmeisenBotManager = new AmeisenBotManager(Dispatcher, OffsetList, Settings);
        }

        #region UIEvents
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewUpdateTimer.Start();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AmeisenBotLogger.Instance.Log("AmeisenBotGui closing...");

            ViewUpdateTimer.Stop();
            AmeisenBotManager.Shutdown();

            AmeisenBotLogger.Instance.Stop();
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void ButtonClose_Click(object sender, RoutedEventArgs e) => Close();

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void ButtonToggleFleet_Click(object sender, RoutedEventArgs e)
        {
            if (AmeisenBotManager.FleetMode)
            {
                AmeisenBotLogger.Instance.Log("FleetMode disabled");
                buttonToggleFleet.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4B4B4B"));
                buttonToggleFleet.Content = "Fleet-Mode OFF";
                AmeisenBotManager.FleetMode = false;
            }
            else
            {
                AmeisenBotLogger.Instance.Log("FleetMode enabled");
                buttonToggleFleet.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078FF"));
                buttonToggleFleet.Content = "Fleet-Mode ON";
                AmeisenBotManager.FleetMode = true;
            }
        }
        #endregion

        #region TimerCallbacks
        private void CUpdateViews(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateBotViews());
            Dispatcher.Invoke(() => UpdateFooterViews());
        }
        #endregion

        private void UpdateBotViews()
        {
            AddNewBotViews();

            foreach (BotView botView in mainWrappanel.Children.OfType<BotView>())
            {
                botView.UpdateView();
            }
            foreach (WowView wowView in mainWrappanel.Children.OfType<WowView>())
            {
                wowView.UpdateView();
            }

            RemoveDeadBotViews();
        }

        private void AddNewBotViews()
        {
            foreach (IAmeisenBotView ameisenBotView in AmeisenBotManager.IAmeisenBotViews)
                if (ameisenBotView.GetType() == typeof(BotView))
                {
                    if (!mainWrappanel.Children.OfType<BotView>().Any(c => ameisenBotView.Process.Id == c.Process.Id))
                        mainWrappanel.Children.Add((BotView)ameisenBotView);
                }
                else if (ameisenBotView.GetType() == typeof(WowView))
                {
                    if (!mainWrappanel.Children.OfType<WowView>().Any(c => ameisenBotView.Process.Id == c.WowProcess.Process.Id))
                        mainWrappanel.Children.Add((WowView)ameisenBotView);
                }
        }

        private void RemoveDeadBotViews()
        {
            List<BotView> botViews = mainWrappanel.Children.OfType<BotView>().ToList();
            foreach (BotView botView in botViews)
            {
                if (!AmeisenBotManager.IAmeisenBotViews.Contains(botView))
                    mainWrappanel.Children.Remove(botView);
            }

            List<WowView> wowViews = mainWrappanel.Children.OfType<WowView>().ToList();
            foreach (WowView wowView in wowViews)
            {
                if (!AmeisenBotManager.IAmeisenBotViews.Contains(wowView))
                    mainWrappanel.Children.Remove(wowView);
            }
        }

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

        private void UpdateFooterViews()
        {
            //labelActiveBotThreads.Content = AmeisenBotManager.AmeisenBotThreads.Count;
            //labelActiveWatchdogs.Content = AmeisenBotManager.AmeisenBotWatchdogThreads.Count;
            labelCurrentMemoryUsage.Content = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1000000;
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

        private void ButtonDebugWindow_Click(object sender, RoutedEventArgs e)
        {
            new DebugWindow().Show();
        }
    }
}
